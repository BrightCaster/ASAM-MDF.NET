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

        public static SourceInformation Read(Mdf mdf, int position)
        {
            mdf.UpdatePosition(position);

            var block = new SourceInformation(mdf);

            block.Read();
            return block;
        }
        internal override void ReadV4()
        {
            base.ReadV4();

           ptrTextName = Mdf.ReadU64().ValidateAddress(Mdf);
           ptrTextPath = Mdf.ReadU64().ValidateAddress(Mdf);
           ptrTextComment = Mdf.ReadU64().ValidateAddress(Mdf);
           SourceType = Mdf.ReadByte();
           BusType = Mdf.ReadByte();
           SiFlags = Mdf.ReadByte();
           Reserved1 = Mdf.ReadByte();

            if (ptrTextComment != 0)
               FileComment = TextBlock.Read(Mdf, (int)ptrTextComment);

            if (ptrTextName != 0)
               TextBlockName = TextBlock.Read(Mdf, (int)ptrTextName);

            if (ptrTextPath != 0)
               TextBlockPath = TextBlock.Read(Mdf, (int)ptrTextPath);
        }
    }
}
