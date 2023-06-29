using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace ASAM.MDF.Libary
{
    public class DataList : Block, INext<DataList>
    {
        private ulong ptrNextDL;
        private ulong ptrDataBlockAddress;
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
            block.ptrDataBlockAddress = mdf.ReadU64();
            block.Flags = (ListFlags)mdf.ReadU16();
            block.Reserved1 = mdf.ReadU16();
            block.BlockCount = mdf.ReadU32();

            if (block.Flags == ListFlags.EqualLength)
                block.ptrDataBlockLen = mdf.ReadU64();
            else 
                block.BlockOffset = mdf.Read64();

            if (block.ptrDataBlockAddress != 0)
                block.DataBlock = DataBlock.Read(mdf, block.ptrDataBlockAddress);

            return block;
        }
    }
}
