using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASAM.MDF.Libary
{
    public class SourceInformation : Block
    {
        private ulong ptrTextName;
        private ulong ptrTextPath;
        private ulong ptrTextComment;

        public byte SourceType { get; private set; }
        public byte BusType { get; private set; }
        public byte SiFlags { get; private set; }
        public byte Reserved1 { get; private set; }
        public TextBlock FileComment { get; private set; }
        public TextBlock TextBlockName { get; private set; }
        public TextBlock TextBlockPath { get; private set; }

        public SourceInformation(Mdf mdf) : base(mdf)
        { }

        public static SourceInformation Read(Mdf mdf, ulong position)
        {
            mdf.UpdatePosition(position);

            var block = new SourceInformation(mdf);
            block.Read();

            block.ptrTextName = mdf.ReadU64();
            block.ptrTextPath = mdf.ReadU64();
            block.ptrTextComment = mdf.ReadU64();
            block.SourceType = mdf.ReadByte();
            block.BusType = mdf.ReadByte();
            block.SiFlags = mdf.ReadByte();
            block.Reserved1 = mdf.ReadByte();

            if (block.ptrTextComment != 0)
                block.FileComment = TextBlock.Read(mdf, block.ptrTextComment);

            if (block.ptrTextName != 0)
                block.TextBlockName = TextBlock.Read(mdf, block.ptrTextName);

            if (block.ptrTextPath != 0)
                block.TextBlockPath = TextBlock.Read(mdf, block.ptrTextPath);

            return block;
        }
    }
}
