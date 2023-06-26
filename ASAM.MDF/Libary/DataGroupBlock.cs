namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class DataGroupBlock : Block, INext<DataGroupBlock>
    {
        private DataGroupBlock nextBlock;
        private ulong ptrNextDataGroup;
        private ulong ptrFirstChannelGroupBlock;
        private ulong ptrTriggerBlock;
        private ulong ptrDataBlock;
        private ulong ptrTextBlock;
        private DataRecord[] records;

        private DataGroupBlock(Mdf mdf) : base(mdf)
        {
            ChannelGroups = new ChannelGroupCollection(mdf, this);
        }

        public DataGroupBlock Next
        {
            get
            {
                if (nextBlock == null && ptrNextDataGroup != 0)
                    nextBlock = Read(Mdf, ptrNextDataGroup);

                return nextBlock;
            }
        }
        public ChannelGroupCollection ChannelGroups { get; private set; }
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

        //public uint Reserved { get; set; }

        public static DataGroupBlock Create(Mdf mdf)
        {
            return new DataGroupBlock(mdf)
            {
                Identifier = "DG"
            };
        }
        public static DataGroupBlock Read(Mdf mdf, ulong position)
        {
            mdf.UpdatePosition(position);

            var block = new DataGroupBlock(mdf);
            block.Read();

            block.nextBlock = null;
            block.Trigger = null;
            block.Reserved = 0;

            if (mdf.IDBlock.Version >= 400)
            {
                block.ptrNextDataGroup = mdf.ReadU64();
                block.ptrFirstChannelGroupBlock = mdf.ReadU64();
                block.ptrDataBlock = mdf.ReadU64();
                block.ptrTextBlock = mdf.ReadU64();
                block.NumRecordIds = mdf.ReadByte();
                block.Reserved1 = mdf.ReadByte();
            }
            else
            {
                block.ptrNextDataGroup = mdf.ReadU32();
                block.ptrFirstChannelGroupBlock = mdf.ReadU32();
                block.ptrTriggerBlock = mdf.ReadU32();
                block.ptrDataBlock = mdf.ReadU32();
                block.NumChannelGroups = mdf.ReadU16();
                block.NumRecordIds = mdf.ReadU16();

                if (block.Size >= 24)
                    block.Reserved = mdf.ReadU32();
            }


            if (block.ptrTextBlock != 0)
                block.FileComment = TextBlock.Read(mdf, block.ptrTextBlock);

            if (block.ptrFirstChannelGroupBlock != 0)
                block.ChannelGroups.Read(ChannelGroupBlock.Read(mdf, block.ptrFirstChannelGroupBlock));

            /// TODO: Call Trigger Blocks
            //if (m_ptrTriggerBlock != 0)
            //{
            //    Mdf.Data.Position = m_ptrTriggerBlock;
            //    Trigger = new TriggerBlock(mdf);
            //}

            /// TODO: Call ProgramsBlock ?
            //if (ptrProgramBlock != 0)
            //{
            //    Mdf.Data.Position = ptrProgramBlock;
            //    ProgramBlock = new ProgramBlock(mdf);
            //}

            block.Records = block.ReadRecords();

            return block;
        }

        internal DataRecord[] ReadRecords()
        {
            var indentificator = Mdf.GetNameBlock(ptrDataBlock);

            if (indentificator == "DZ")
                DataZippedBlock.Read(Mdf, ptrDataBlock);

            Mdf.UpdatePosition(ptrDataBlock);

            var recordsList = new List<DataRecord>();

            if (Mdf.IDBlock.Version >= 400)
            {
                for (int i = 0; i < ChannelGroups.Count; i++)
                {
                    var group = ChannelGroups[i];

                    for (int k = 0; k < (int)group.CycleCount; k++)
                    {
                        var recordData = Mdf.ReadBytes((ushort)group.DataBytes);

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

        internal override ushort GetSize()
        {
            return 28;
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
    }
}
