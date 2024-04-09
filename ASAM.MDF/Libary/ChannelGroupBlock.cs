namespace ASAM.MDF.Libary
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    public class ChannelGroupBlock : Block, INext<ChannelGroupBlock>, IPrevious<ChannelGroupBlock>, IParent<DataGroupBlock>
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
            Channels = new ChannelCollection(mdf, this);
        }

        public ChannelGroupBlock Next
        {
            get
            {
                if (next == null && ptrNextChannelGroup != 0)
                    next = Read(Mdf, (int)ptrNextChannelGroup);

                return next;
            }
        }

        public ChannelGroupBlock Previous { get; set; }
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
        DataGroupBlock IParent<DataGroupBlock>.Parent { get; set; }

        public static ChannelGroupBlock Create(Mdf mdf)
        {
            return new ChannelGroupBlock(mdf) { Identifier = "CG" };
        }

        internal static ChannelGroupBlock Read(Mdf mdf, int position)
        {
            mdf.UpdatePosition(position);

            var block = new ChannelGroupBlock(mdf);
            block.next = null;
            block.Comment = null;
            block.SampleReductions = null;

            block.Read();

            return block;
        }
        internal override void ReadV23()
        {
            base.ReadV23();

            ptrNextChannelGroup = Mdf.ReadU32().ValidateAddress(Mdf);
            ptrFirstChannelBlock = Mdf.ReadU32().ValidateAddress(Mdf);
            ptrTextBlock = Mdf.ReadU32().ValidateAddress(Mdf);
            RecordID = Mdf.ReadU16();
            NumChannels = Mdf.ReadU16();
            RecordSize = Mdf.ReadU16();
            NumRecords = Mdf.ReadU32();

            if (Size >= 26)
                ptrFirstSampleReductionBlock = Mdf.ReadU32();

            if (ptrTextBlock != 0)
                Comment = TextBlock.Read(Mdf, (int)ptrTextBlock);

            if (ptrTextName != 0)
                TextName = TextBlock.Read(Mdf, (int)ptrTextName);

            if (ptrFirstChannelBlock != 0)
            {
                var chBlock = ChannelBlock.Read(Mdf, (int)ptrFirstChannelBlock);
                chBlock.ChanelsRemovedAddress += (ch, bytes) => ChBlock_ChanelsRemovedAddress(ch, bytes);

                Channels.Read(chBlock, ChBlock_ChanelsRemovedAddress);
            }
        }
        internal override void ReadV4()
        {
            base.ReadV4();

            ptrNextChannelGroup = Mdf.ReadU64().ValidateAddress(Mdf);
            ptrFirstChannelBlock = Mdf.ReadU64().ValidateAddress(Mdf);
            ptrTextName = Mdf.ReadU64().ValidateAddress(Mdf);
            ptrSourceInfo = Mdf.ReadU64().ValidateAddress(Mdf);
            ptrFirstSampleReductionBlock = Mdf.ReadU64().ValidateAddress(Mdf);
            ptrTextBlock = Mdf.ReadU64().ValidateAddress(Mdf);
            RecordID = Mdf.ReadU64();
            CycleCount = Mdf.ReadU64();
            Flags = Mdf.ReadU16();
            pathSeparator = Mdf.ReadChar();
            Reserved1 = Mdf.ReadU32();
            DataBytes = Mdf.ReadU32();
            InvalidBytes = Mdf.ReadU32();

            if (ptrTextBlock != 0)
                Comment = TextBlock.Read(Mdf, (int)ptrTextBlock);

            if (ptrTextName != 0)
                TextName = TextBlock.Read(Mdf, (int)ptrTextName);

            if (ptrFirstChannelBlock != 0)
                Channels.Read(ChannelBlock.Read(Mdf, (int)ptrFirstChannelBlock), null);
        }
        private void ChBlock_ChanelsRemovedAddress(ChannelBlock block, byte[] bytes)
        {
            if (Channels[0] == block) //change first channel address on channelGroup
            {
                var nextBlock = block.Next;
                if (nextBlock == null)
                    ptrFirstChannelBlock = 0;
                else
                    ptrFirstChannelBlock = (ulong)block.Next.BlockAddress;

                var addressOfFirstChannel = BlockAddress + 4 + 4/*ptrNextChannelGroup*/;
                var bytesFirstChannelAddress = BitConverter.GetBytes(ptrFirstChannelBlock);
                Array.Copy(bytesFirstChannelAddress, 0, bytes, addressOfFirstChannel, bytesFirstChannelAddress.Length);
            }
            if (block.Previous == null && block.Next == null)
            {
                ptrFirstChannelBlock = 0;
                var addressOfFirstChannel = BlockAddress + 4 + 4/*ptrNextChannelGroup*/;
                var bytesFirstChannelAddress = BitConverter.GetBytes(ptrFirstChannelBlock);
                Array.Copy(bytesFirstChannelAddress, 0, bytes, addressOfFirstChannel, bytesFirstChannelAddress.Length);
            }
            NumChannels -= 1;

            var addressNumChannels = BlockAddress + 4 + 4/*ptrNextChannelGroup*/ + 4/*ptrFirstChannelBlock*/ + 4/*ptrTextBlock*/ + 2/*RecordID*/;
            var newbytes = BitConverter.GetBytes(NumChannels);
            Array.Copy(newbytes, 0, bytes, addressNumChannels, newbytes.Length);
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
