namespace ASAM.MDF.Libary
{
    using System;
    using System.IO;

    public class ChannelGroupBlock : Block, INext<ChannelGroupBlock>
    {
        private ulong ptrNextChannelGroup;
        private ulong ptrFirstChannelBlock;
        private ulong ptrTextName;
        private ulong ptrTextBlock;
        private ulong ptrSourceInfo;
        private char pathSeparator;

        private ulong ptrFirstSampleReductionBlock;
        private ChannelGroupBlock next;

        private ChannelGroupBlock(Mdf mdf) : base(mdf)
        {
        }

        public ChannelGroupBlock Next
        {
            get
            {
                if (next == null && ptrNextChannelGroup != 0)
                    next = Read(Mdf, ptrNextChannelGroup);

                return next;
            }
        }

        public ChannelCollection Channels { get; private set; }
        public TextBlock Comment { get; set; }
        public ulong RecordID { get; private set; }
        public ulong CycleCount { get; private set; }
        public ushort Flags { get; private set; }


        public uint Reserved1 { get; private set; }
        public uint DataBytes { get; private set; }
        public uint InvalidBytes { get; private set; }
        public ushort NumChannels { get; private set; }
        public ushort RecordSize { get; set; }
        public uint NumRecords { get; set; }
        public SampleReductionCollection SampleReductions { get; private set; }
        public TextBlock TextName { get; private set; }

        public static ChannelGroupBlock Create(Mdf mdf)
        {
            return new ChannelGroupBlock(mdf)
            {
                Channels = new ChannelCollection(mdf),
                Identifier = "CG",
            };
        }

        internal static ChannelGroupBlock Read(Mdf mdf, ulong position)
        {
            mdf.UpdatePosition(position);

            var block = new ChannelGroupBlock(mdf);
            block.Read();

            block.next = null;
            block.Channels = new ChannelCollection(mdf);
            block.Comment = null;
            block.SampleReductions = null;

            if (mdf.IDBlock.Version >= 400)
            {
                ReadV4(mdf, block);
                return block;
            }
            block.ptrNextChannelGroup = mdf.ReadU32().ValidateAddress(mdf);
            block.ptrFirstChannelBlock = mdf.ReadU32().ValidateAddress(mdf);
            block.ptrTextBlock = mdf.ReadU32().ValidateAddress(mdf);
            block.RecordID = mdf.ReadU16();
            block.NumChannels = mdf.ReadU16();
            block.RecordSize = mdf.ReadU16();
            block.NumRecords = mdf.ReadU32();

            if (block.Size >= 26)
                block.ptrFirstSampleReductionBlock = mdf.ReadU32();

            if (block.ptrTextBlock != 0)
                block.Comment = TextBlock.Read(mdf, block.ptrTextBlock);

            if (block.ptrTextName != 0)
                block.TextName = TextBlock.Read(mdf, block.ptrTextName);

            if (block.ptrFirstChannelBlock != 0)
                block.Channels.Read(ChannelBlock.Read(mdf, block.ptrFirstChannelBlock));

            //if (m_ptrFirstSampleReductionBlock != 0)
            //{
            //    mdf.Data.Position = m_ptrFirstSampleReductionBlock;
            //    SampleReductions = new SampleReductionCollection(mdf, new SampleReductionBlock(mdf));
            //}

            return block;
        }

        private static void ReadV4(Mdf mdf, ChannelGroupBlock block)
        {
            block.ptrNextChannelGroup = mdf.ReadU64().ValidateAddress(mdf);
            block.ptrFirstChannelBlock = mdf.ReadU64().ValidateAddress(mdf);
            block.ptrTextName = mdf.ReadU64().ValidateAddress(mdf);
            block.ptrSourceInfo = mdf.ReadU64().ValidateAddress(mdf);
            block.ptrFirstSampleReductionBlock = mdf.ReadU64().ValidateAddress(mdf);
            block.ptrTextBlock = mdf.ReadU64().ValidateAddress(mdf);
            block.RecordID = mdf.ReadU64();
            block.CycleCount = mdf.ReadU64();
            block.Flags = mdf.ReadU16();
            block.pathSeparator = mdf.ReadChar();
            block.Reserved1 = mdf.ReadU32();
            block.DataBytes = mdf.ReadU32();
            block.InvalidBytes = mdf.ReadU32();

            if (block.ptrTextBlock != 0)
                block.Comment = TextBlock.Read(mdf, block.ptrTextBlock);

            if (block.ptrTextName != 0)
                block.TextName = TextBlock.Read(mdf, block.ptrTextName);

            if (block.ptrFirstChannelBlock != 0)
                block.Channels.Read(ChannelBlock.Read(mdf, block.ptrFirstChannelBlock));
        }

        internal override ushort GetSize()
        {
            return 30;
        }
        internal override int GetSizeTotal()
        {
            var size = base.GetSizeTotal();

            for (int i = 0; i < Channels.Count; i++)
                size += Channels[i].GetSizeTotal();

            size += Comment.GetSizeSafe();

            return size;
        }
        internal override void Write(byte[] array, ref int index)
        {
            base.Write(array, ref index);

            var bytesRecordId = BitConverter.GetBytes(RecordID);
            var bytesNumChannels = BitConverter.GetBytes(Channels.Count);
            var bytesRecordSize = BitConverter.GetBytes(RecordSize);
            var bytesNumRecords = BitConverter.GetBytes(NumRecords);

            Array.Copy(bytesRecordId, 0, array, index + 16, bytesRecordId.Length);
            Array.Copy(bytesNumChannels, 0, array, index + 18, bytesNumChannels.Length);
            Array.Copy(bytesRecordSize, 0, array, index + 20, bytesRecordSize.Length);
            Array.Copy(bytesNumRecords, 0, array, index + 22, bytesNumRecords.Length);

            index += GetSize();
        }
        internal void WriteChannels(byte[] array, ref int index, int blockIndex)
        {
            if (Channels.Count == 0)
                return;

            var bytesFirstChannelLink = BitConverter.GetBytes(index);

            Array.Copy(bytesFirstChannelLink, 0, array, blockIndex + 8, bytesFirstChannelLink.Length);

            Channels.Write(array, ref index);
        }
        internal void WriteComment(byte[] array, ref int index, int blockIndex)
        {
            if (Comment == null)
                return;

            var bytesCommentLink = BitConverter.GetBytes(index);

            Array.Copy(bytesCommentLink, 0, array, blockIndex + 12, bytesCommentLink.Length);

            Comment.Write(array, ref index);
        }
        internal void WriteNextChannelGroupBlockLink(byte[] array, int index, int blockIndex)
        {
            var bytesNextChannelGroupBlockLink = BitConverter.GetBytes(index);

            Array.Copy(bytesNextChannelGroupBlockLink, 0, array, blockIndex + 4, bytesNextChannelGroupBlockLink.Length);
        }
        public override Block Clone(Mdf mdf)
        {
            var cgb = base.Clone(mdf) as ChannelGroupBlock;
            cgb.Channels = Channels?.Clone(mdf);
            cgb.next = next?.Clone(mdf) as ChannelGroupBlock;
            cgb.Comment = Comment?.Clone(mdf) as TextBlock;
            cgb.TextName = TextName?.Clone(mdf) as TextBlock;

            return cgb;
        }
    }
}
