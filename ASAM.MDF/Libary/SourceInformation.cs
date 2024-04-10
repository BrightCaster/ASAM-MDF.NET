namespace ASAM.MDF.Libary
{
    public class SourceInformation : Block
    {
        internal (ulong address, int offset) ptrTextName;
        internal (ulong address, int offset) ptrTextPath;
        internal (ulong address, int offset) ptrTextComment;

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

            ptrTextName = (Mdf.ReadU64().ValidateAddress(Mdf), 24);
            ptrTextPath = (Mdf.ReadU64().ValidateAddress(Mdf), ptrTextName.offset + 8);
            ptrTextComment = (Mdf.ReadU64().ValidateAddress(Mdf), ptrTextPath.offset + 8);
            SourceType = Mdf.ReadByte();
            BusType = Mdf.ReadByte();
            SiFlags = Mdf.ReadByte();
            Reserved1 = Mdf.ReadByte();

             if (ptrTextComment.address != 0)
                FileComment = TextBlock.Read(Mdf, (int)ptrTextComment.address);

             if (ptrTextName.address != 0)
                TextBlockName = TextBlock.Read(Mdf, (int)ptrTextName.address);

             if (ptrTextPath.address != 0)
                TextBlockPath = TextBlock.Read(Mdf, (int)ptrTextPath.address);
        }
    }
}
