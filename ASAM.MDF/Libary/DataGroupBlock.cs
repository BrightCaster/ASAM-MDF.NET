﻿namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;

    public class DataGroupBlock : Block, INext<DataGroupBlock>, IPrevious<DataGroupBlock>, IParent<Mdf>
    {
        internal List<PointerAddress<uint>> listAddressesV23;
        internal List<PointerAddress<ulong>> listAddressesV4;

        internal PointerAddress<uint> ptrNextDataGroup;
        internal PointerAddress<uint> ptrFirstChannelGroupBlock;
        internal PointerAddress<uint> ptrTriggerBlock;
        internal PointerAddress<uint> ptrDataBlock;

        internal PointerAddress<ulong> ptrNextDataGroupV4;
        internal PointerAddress<ulong> ptrFirstChannelGroupBlockV4;
        internal PointerAddress<ulong> ptrTriggerBlockV4;
        internal PointerAddress<ulong> ptrDataBlockV4;
        internal PointerAddress<ulong> ptrTextBlockV4;

        private DataRecord[] records;
        private DataGroupBlock nextBlock;

        private DataGroupBlock(Mdf mdf) : base(mdf)
        {
            ChannelGroups = new ChannelGroupCollection(mdf, this);
            DataListColl = new DataListCollection(mdf, this);
        }

        public DataGroupBlock Next
        {
            get
            {
                if (Mdf.IDBlock.Version >= 400)
                {
                    if (nextBlock == null && ptrNextDataGroupV4 != null && ptrNextDataGroupV4?.address != 0)
                        nextBlock = Read(Mdf, (int)ptrNextDataGroupV4.address);
                }
                else if (nextBlock == null && ptrNextDataGroup != null && ptrNextDataGroup.address != 0)
                    nextBlock = Read(Mdf, (int)ptrNextDataGroup.address);

                return nextBlock;
            }
        }
        public DataGroupBlock Previous { get; set; }
        public ChannelGroupCollection ChannelGroups { get; private set; }
        public DataListCollection DataListColl { get; private set; }
        public TriggerBlock Trigger { get; set; }

        public ushort NumChannelGroups { get; private set; }
        /// <summary>
        /// Number of record IDs in the data block
        /// 0 = data   records without  record ID
        /// 1 = record ID(UINT8) before each data record
        /// 2 = record ID(UINT8) before and after each data record
        /// </summary>
        public ushort NumRecordIds { get; private set; }
        public byte Reserved1 { get; private set; }

        public DataRecord[] Records
        {
            get
            {
                return records;
            }
            set { records = value; }
        }

        public TextBlock FileComment { get; private set; }
        internal DataZippedBlock DataZipped { get; private set; }
        public Mdf Parent { get; set; }

        //public uint Reserved { get; set; }

        public static DataGroupBlock Create(Mdf mdf)
        {
            return new DataGroupBlock(mdf)
            {
                Identifier = "DG"
            };
        }
        public static DataGroupBlock Read(Mdf mdf, int position)
        {
            mdf.UpdatePosition(position);

            var block = new DataGroupBlock(mdf);
            block.nextBlock = null;
            block.Trigger = null;
            block.Reserved = 0;

            block.Read();
            block.Records = block.ReadRecords();

            return block;
        }
        internal override void ReadV23()
        {
            base.ReadV23();

            listAddressesV23 = new List<PointerAddress<uint>>();

            ptrNextDataGroup = new PointerAddress<uint>(Mdf.ReadU32().ValidateAddress(Mdf), 4);
            ptrFirstChannelGroupBlock = new PointerAddress<uint>(Mdf.ReadU32().ValidateAddress(Mdf), ptrNextDataGroup.offset + 4);
            ptrTriggerBlock = new PointerAddress<uint>(Mdf.ReadU32().ValidateAddress(Mdf), ptrFirstChannelGroupBlock.offset + 4);
            ptrDataBlock = new PointerAddress<uint>(Mdf.ReadU32().ValidateAddress(Mdf), ptrTriggerBlock.offset + 4);
            NumChannelGroups = Mdf.ReadU16();
            NumRecordIds = Mdf.ReadU16();

            if (Size >= 24)
                Reserved = Mdf.ReadU32();

            if (ptrFirstChannelGroupBlock.address != 0)
                ChannelGroups.Read(ChannelGroupBlock.Read(Mdf, (int)ptrFirstChannelGroupBlock.address));

            listAddressesV23.AddRange(new PointerAddress<uint>[]
            {
                ptrNextDataGroup,
                ptrFirstChannelGroupBlock,
                ptrTriggerBlock,
                ptrDataBlock,
            });

            /// TODO: Call Trigger Blocks
            //if (m_ptrTriggerBlock != 0)
            //{
            //    Mdf.Data.Position = m_ptrTriggerBlock;
            //    Trigger = new TriggerBlock(Mdf);
            //}

            /// TODO: Call ProgramsBlock ?
            //if (ptrProgramBlock != 0)
            //{
            //    Mdf.Data.Position = ptrProgramBlock;
            //    ProgramBlock = new ProgramBlock(Mdf);
            //}
        }
        internal override void ReadV4()
        {
            base.ReadV4();

            listAddressesV4 = new List<PointerAddress<ulong>>();

            ptrNextDataGroupV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), 24);
            ptrFirstChannelGroupBlockV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrNextDataGroupV4.offset + 8);
            ptrDataBlockV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf),ptrFirstChannelGroupBlockV4.offset + 8);
            ptrTextBlockV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrDataBlockV4.offset + 8);
            NumRecordIds = Mdf.ReadByte();
            Reserved1 = Mdf.ReadByte();

            if (ptrTextBlockV4.address != 0)
                FileComment = TextBlock.Read(Mdf, (int)ptrTextBlockV4.address);

            if (ptrFirstChannelGroupBlockV4.address != 0)
                ChannelGroups.Read(ChannelGroupBlock.Read(Mdf, (int)ptrFirstChannelGroupBlockV4.address));

            if (ptrDataBlockV4.address != 0)
            {
                var indentificator = Mdf.GetNameBlock((int)ptrDataBlockV4.address);

                if (indentificator == "DZ")
                    DataZipped = DataZippedBlock.Read(Mdf, (int)ptrDataBlockV4.address);

                if (indentificator == "DL")
                {
                    DataListColl.Read(DataList.Read(Mdf, (int)ptrDataBlockV4.address));
                }
            }

            listAddressesV4.AddRange(new PointerAddress<ulong>[]
            {
                ptrNextDataGroupV4,
                ptrDataBlockV4,
                ptrFirstChannelGroupBlockV4,
                ptrTextBlockV4,
            });
        }

        internal DataRecord[] ReadRecords()
        {
            var recordsList = new List<DataRecord>();
            var dataList = new List<DataBlock>();

            var indentificator = Mdf.GetNameBlock((int)ptrDataBlock.address);

            if (Mdf.IDBlock.Version >= 400)
                indentificator = Mdf.GetNameBlock((int)ptrDataBlockV4.address);

            if (indentificator == "DL")
            {
                for (int i = 0; i < DataListColl.Count; i++)
                {
                    dataList.Add(DataListColl[i].DataBlock);
                }
            }

            Mdf.UpdatePosition((int)ptrDataBlock.address);

            if (Mdf.IDBlock.Version >= 400)
                Mdf.UpdatePosition((int)ptrDataBlockV4.address);//

            if (Mdf.IDBlock.Version >= 400)
            {
                if (dataList.Count != 0)
                    for (int j = 0; j < dataList.Count; j++)
                    {
                        var data = dataList[j];
                        var position = 0;

                        for (int i = 0; i < ChannelGroups.Count; i++)
                        {
                            var group = ChannelGroups[i];

                            for (int k = 0; k < (int)group.CycleCount; k++)
                            {
                                var recordData = Mdf.ReadBytes(data.DataOfBlock, (int)group.DataBytes + (int)group.InvalidBytes, ref position);

                                recordsList.Add(new DataRecord(group, recordData));
                            }
                        }
                    }
                else
                    for (int i = 0; i < ChannelGroups.Count; i++)
                    {
                        var group = ChannelGroups[i];

                        for (int k = 0; k < (int)group.CycleCount; k++)
                        {
                            var recordData = Mdf.ReadBytes((int)group.DataBytes);

                            recordsList.Add(new DataRecord(group, recordData));
                        }
                    }

            }
            else
            {
                for (int i = 0; i < NumChannelGroups; i++)
                {
                    var group = ChannelGroups[i];

                    for (int k = 0; k < group.NumRecords; k++)
                    {
                        var recordData = Mdf.ReadBytes(group.RecordSize);

                        recordsList.Add(new DataRecord(group, recordData));
                    }
                }
            }

            return recordsList.ToArray();
        }

        internal override int GetSizeTotal()
        {
            var size = base.GetSizeTotal();

            for (int i = 0; i < ChannelGroups.Count; i++)
                size += ChannelGroups[i].GetSizeTotal();

            if (records != null)
                for (int i = 0; i < records.Length; i++)
                {
                    var r = records[i];
                    if (r.Data == null)
                        continue;

                    size += r.Data.Length;
                }

            return size;
        }
        internal override void Write(byte[] array, ref int index)
        {
            base.Write(array, ref index);

            var bytesNumChannelGroups = BitConverter.GetBytes(ChannelGroups.Count);
            var bytesNumRecordsIds = BitConverter.GetBytes(NumRecordIds);
            var bytesReserved = BitConverter.GetBytes(Reserved);

            Array.Copy(bytesNumChannelGroups, 0, array, index + 20, bytesNumChannelGroups.Length);
            Array.Copy(bytesNumRecordsIds, 0, array, index + 22, bytesNumRecordsIds.Length);
            Array.Copy(bytesReserved, 0, array, index + 24, bytesReserved.Length);

            index += GetSize();
        }
        internal void WriteChannelGroups(byte[] array, ref int index)
        {
            ChannelGroups.Write(array, ref index);
        }
        internal void WriteFirstChannelGroupBlockLink(byte[] array, int index, int blockIndex)
        {
            var bytesFirst = BitConverter.GetBytes(index);

            Array.Copy(bytesFirst, 0, array, blockIndex + 8, bytesFirst.Length);
        }
        internal void WriteNextBlockLink(byte[] array, int index, int blockIndex)
        {
            var bytesNextBlockLink = BitConverter.GetBytes(index);

            Array.Copy(bytesNextBlockLink, 0, array, blockIndex + 4, bytesNextBlockLink.Length);
        }
        internal void WriteRecords(byte[] array, ref int index, int blockIndex)
        {
            if (records == null || records.Length == 0)
                return;

            var bytesRecordsLink = BitConverter.GetBytes(index);

            Array.Copy(bytesRecordsLink, 0, array, blockIndex + 16, bytesRecordsLink.Length);

            for (int i = 0; i < records.Length; i++)
            {
                var r = records[i];
                if (r.Data == null)
                    continue;

                Array.Copy(r.Data, 0, array, index, r.Data.Length);

                index += r.Data.Length;
            }
        }

        internal void DataGroupUpdateAddress(int indexDeleted, byte[] bytes, ulong countDeleted)
        {
            if (Mdf.IDBlock.Version >= 400)
                DataGroupUpdateAddressV4(indexDeleted, bytes, countDeleted);
            else
                DataGroupUpdateAddressV23(indexDeleted, bytes, (uint)countDeleted);
        }

        private void DataGroupUpdateAddressV23(int indexDeleted, byte[] bytes, uint countDeleted)
        {
            foreach (var ptr in listAddressesV23)
            {
                if ((int)ptr.address > indexDeleted)
                {
                    ptr.address -= countDeleted;

                    this.CopyAddress(ptr, bytes);
                }
            }
        }

        private void DataGroupUpdateAddressV4(int indexDeleted, byte[] bytes, ulong countDeleted)
        {
            foreach (var ptr in listAddressesV4)
            {
                if ((int)ptr.address > indexDeleted)
                {
                    ptr.address -= countDeleted;

                    this.CopyAddress(ptr, bytes);
                }
            }
        }
    }
}
