namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;

    using global::Mdf;

    public class DataGroupBlock : Block, INext<DataGroupBlock>
    {
        private DataGroupBlock m_Next;
        private uint m_ptrNextDataGroup;
        private uint m_ptrFirstChannelGroupBlock;
        private uint m_ptrTriggerBlock;
        public uint m_ptrDataBlock;

        public DataGroupBlock(Mdf mdf) : base(mdf)
        {
            var data = new byte[Size - 4];
            var read = Mdf.Data.Read(data, 0, data.Length);

            if (read != data.Length)
                throw new FormatException();

            m_Next = null;
            ChannelGroups = null;
            Trigger = null;
            Reserved = 0;

            m_ptrNextDataGroup = BitConverter.ToUInt32(data, 0);
            m_ptrFirstChannelGroupBlock = BitConverter.ToUInt32(data, 4);
            m_ptrTriggerBlock = BitConverter.ToUInt32(data, 8);
            m_ptrDataBlock = BitConverter.ToUInt32(data, 12);
            NumChannelGroups = BitConverter.ToUInt16(data, 16);
            NumRecordIds = BitConverter.ToUInt16(data, 18);

            if (data.Length >= 24)
                Reserved = BitConverter.ToUInt32(data, 20);

            Mdf.Data.Position = m_ptrFirstChannelGroupBlock;

            ChannelGroups = new ChannelGroupCollection(mdf, new ChannelGroupBlock(mdf));

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
        }

        public DataGroupBlock Next
        {
            get
            {
                if (m_Next == null && m_ptrNextDataGroup != 0)
                {
                    Mdf.Data.Position = m_ptrNextDataGroup;
                    m_Next = new DataGroupBlock(Mdf);
                }

                return m_Next;
            }
        }
        public ChannelGroupCollection ChannelGroups { get; private set; }
        public DataRecord[] Records
        {
            get
            {
                Mdf.Data.Position = m_ptrDataBlock;

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
        public uint Reserved { get; private set; }
    }
}
