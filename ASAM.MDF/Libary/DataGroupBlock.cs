namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class DataGroupBlock : Block, INext<DataGroupBlock>
    {
        private Stream stream;
        private DataGroupBlock nextBlock;
        private uint ptrNextDataGroup;
        private uint ptrFirstChannelGroupBlock;
        private uint ptrTriggerBlock;
        private uint ptrDataBlock;

        private DataGroupBlock(Mdf mdf) : base(mdf)
        {
            ChannelGroups = new ChannelGroupCollection(mdf, this);
        }

        public DataGroupBlock Next
        {
            get
            {
                if (nextBlock == null && ptrNextDataGroup != 0)
                    nextBlock = Read(Mdf, stream, ptrNextDataGroup);

                return nextBlock;
            }
        }
        public ChannelGroupCollection ChannelGroups { get; private set; }
        public DataRecord[] Records
        {
            get
            {
                Mdf.Data.Position = ptrDataBlock;

                var records = new List<DataRecord>();

                for (int i = 0; i < NumChannelGroups; i++)
                {
                    var group = ChannelGroups[i];
                    for (int k = 0; k < group.NumRecords; k++)
                    {
                        var recordData = new byte[group.RecordSize];
                        var read = Mdf.Data.Read(recordData, 0, recordData.Length);
                        if (read != recordData.Length)
                            throw new FormatException();

                        records.Add(new DataRecord(group, recordData));
                    }
                }

                return records.ToArray();
            }
        }
        public TriggerBlock Trigger { get; set; }
        
        public ushort NumChannelGroups { get; private set; }
        /// <summary>
        /// Number of record IDs in the data block
        /// 0 = data   records without  record ID
        /// 1 = record ID(UINT8) before each data record
        /// 2 = record ID(UINT8) before and after each data record
        /// </summary>
        public ushort NumRecordIds { get; private set; }
        public uint Reserved { get; set; }

        public static DataGroupBlock Create(Mdf mdf)
        {
            return new DataGroupBlock(mdf)
            {
                Identifier = "DG"
            };
        }
        public static DataGroupBlock Read(Mdf mdf, Stream stream, uint position)
        {
            stream.Position = position;

            var block = new DataGroupBlock(mdf);
            block.Read(stream);
            block.stream = stream;

            var data = new byte[block.Size - 4];
            var read = stream.Read(data, 0, data.Length);

            if (read != data.Length)
                throw new FormatException();

            block.nextBlock = null;
            block.Trigger = null;
            block.Reserved = 0;

            block.ptrNextDataGroup = BitConverter.ToUInt32(data, 0);
            block.ptrFirstChannelGroupBlock = BitConverter.ToUInt32(data, 4);
            block.ptrTriggerBlock = BitConverter.ToUInt32(data, 8);
            block.ptrDataBlock = BitConverter.ToUInt32(data, 12);
            block.NumChannelGroups = BitConverter.ToUInt16(data, 16);
            block.NumRecordIds = BitConverter.ToUInt16(data, 18);

            if (data.Length >= 24)
                block.Reserved = BitConverter.ToUInt32(data, 20);

            if (block.ptrFirstChannelGroupBlock != 0)
                block.ChannelGroups.Read(ChannelGroupBlock.Read(mdf, stream, block.ptrFirstChannelGroupBlock));

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

            return block;
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
    }
}
