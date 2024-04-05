﻿namespace ASAM.MDF.Libary
{
    public class DataList : Block, INext<DataList>
    {
        private ulong ptrNextDL;

        public ulong DataBlockAddress { get; private set; }

        private ulong ptrDataBlockLen;
        private DataList nextBlock;

        public ListFlags Flags { get; private set; }
        public ushort Reserved1 { get; private set; }
        public uint BlockCount { get; private set; }
        public long BlockOffset { get; private set; }
        public DataList(Mdf mdf) : base(mdf)
        {
        }
        public DataList Next
        {
            get
            {
                if (nextBlock == null && ptrNextDL != 0)
                    nextBlock = Read(Mdf, ptrNextDL);

                return nextBlock;
            }
        }

        public DataBlock DataBlock { get; private set; }

        public static DataList Read(Mdf mdf, ulong position)
        {
            mdf.UpdatePosition(position);

            var block = new DataList(mdf);
            block.Read();

            block.ptrNextDL = mdf.ReadU64();
            block.DataBlockAddress = mdf.ReadU64();
            block.Flags = (ListFlags)mdf.ReadU16();
            block.Reserved1 = mdf.ReadU16();
            block.BlockCount = mdf.ReadU32();

            if (block.Flags == ListFlags.EqualLength)
                block.ptrDataBlockLen = mdf.ReadU64();
            else 
                block.BlockOffset = mdf.Read64();

            if (block.DataBlockAddress != 0)
                block.DataBlock = DataBlock.Read(mdf, block.DataBlockAddress);

            return block;
        }
        public override Block Clone(Mdf mdf)
        {
            var dl = base.Clone(mdf) as DataList;
            dl.DataBlock = DataBlock?.Clone(mdf) as DataBlock;
            dl.nextBlock = nextBlock?.Clone(mdf) as DataList;

            return dl;
        }
    }
}
