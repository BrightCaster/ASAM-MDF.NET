﻿using System;
using System.Security.Cryptography;

namespace ASAM.MDF.Libary
{
    public class DataList : Block, INext<DataList>, IPrevious<DataList>, IParent<DataGroupBlock>
    {
        internal PointerAddress<ulong> ptrNextDLV4;
        internal PointerAddress<ulong> ptrDataBlockAddressV4;

        private ulong ptrDataBlockLen;
        private DataList nextBlock;

        public ListFlags Flags { get; private set; }
        public ushort Reserved1 { get; private set; }
        public uint BlockCount { get; private set; }
        public long BlockOffset { get; private set; }
        public DataList(Mdf mdf) : base(mdf)
        { }

        public DataList Next
        {
            get
            {
                if (nextBlock == null && ptrNextDLV4.address != 0)
                    nextBlock = Read(Mdf, (int)ptrNextDLV4.address);

                return nextBlock;
            }
        }
        public DataList Previous { get; set; }

        public DataBlock DataBlock { get; private set; }
        public DataGroupBlock Parent { get; set; }

        public static DataList Read(Mdf mdf, int position)
        {
            mdf.UpdatePosition(position);

            var block = new DataList(mdf);
            block.Read();

            return block;
        }
        internal override void ReadV4()
        {
            base.ReadV4();

            ptrNextDLV4 =  new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), 24);
            ptrDataBlockAddressV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf),ptrNextDLV4.offset + 8);
            Flags = (ListFlags)Mdf.ReadU16();
            Reserved1 = Mdf.ReadU16();
            BlockCount = Mdf.ReadU32();

            if (Flags == ListFlags.EqualLength)
                ptrDataBlockLen = Mdf.ReadU64();
            else
                BlockOffset = Mdf.Read64();

            if (ptrDataBlockAddressV4.address != 0)
                DataBlock = DataBlock.Read(Mdf, (int)ptrDataBlockAddressV4.address);
        }
    }
}
