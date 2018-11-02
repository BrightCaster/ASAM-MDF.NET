namespace ASAM.MDF.Libary
{
    using System;
    using System.IO;

    public class ChannelGroupBlock : Block, INext<ChannelGroupBlock>
    {
        private uint ptrNextChannelGroup;
        private uint ptrFirstChannelBlock;
        private uint ptrTextBlock;
        private uint ptrFirstSampleReductionBlock;
        private ChannelGroupBlock next;
        private Stream stream;

        private ChannelGroupBlock(Mdf mdf) : base(mdf)
        {
        }

        public ChannelGroupBlock Next
        {
            get
            {
                if (next == null && ptrNextChannelGroup != 0)
                    next = Read(Mdf, stream, ptrNextChannelGroup);

                return next;
            }
        }

        public ChannelCollection Channels { get; private set; }
        public TextBlock Comment { get; set; }
        public ushort RecordID { get; private set; }
        public ushort NumChannels { get; private set; }
        public ushort RecordSize { get; set; }
        public uint NumRecords { get; set; }
        public SampleReductionCollection SampleReductions { get; private set; }

        public static ChannelGroupBlock Create(Mdf mdf)
        {
            return new ChannelGroupBlock(mdf)
            {
                Channels = new ChannelCollection(mdf),
                Identifier = "CG",
            };
        }

        internal static ChannelGroupBlock Read(Mdf mdf, Stream stream, uint position)
        {
            stream.Position = position;

            var block = new ChannelGroupBlock(mdf);
            block.Read(stream);
            block.stream = stream;

            var data = new byte[block.Size - 4];
            var read = stream.Read(data, 0, data.Length);

            if (read != data.Length)
                throw new FormatException();

            block.next = null;
            block.Channels = new ChannelCollection(mdf);
            block.Comment = null;
            block.SampleReductions = null;

            block.ptrNextChannelGroup = BitConverter.ToUInt32(data, 0);
            block.ptrFirstChannelBlock = BitConverter.ToUInt32(data, 4);
            block.ptrTextBlock = BitConverter.ToUInt32(data, 8);
            block.RecordID = BitConverter.ToUInt16(data, 12);
            block.NumChannels = BitConverter.ToUInt16(data, 14);
            block.RecordSize = BitConverter.ToUInt16(data, 16);
            block.NumRecords = BitConverter.ToUInt32(data, 18);

            if (data.Length >= 26)
                block.ptrFirstSampleReductionBlock = BitConverter.ToUInt32(data, 22);

            if (block.ptrTextBlock != 0)
            {
                stream.Position = block.ptrTextBlock;
                block.Comment = TextBlock.Read(mdf, stream);
            }

            if (block.ptrFirstChannelBlock != 0)
                block.Channels.Read(ChannelBlock.Read(mdf, stream, block.ptrFirstChannelBlock));

            //if (m_ptrFirstSampleReductionBlock != 0)
            //{
            //    mdf.Data.Position = m_ptrFirstSampleReductionBlock;
            //    SampleReductions = new SampleReductionCollection(mdf, new SampleReductionBlock(mdf));
            //}

            return block;
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
    }
}
