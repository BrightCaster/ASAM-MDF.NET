namespace ASAM.MDF.Libary
{
    public class DataBlock : Block
    {
        public DataBlock(Mdf mdf) : base(mdf)
        { }

        public byte[] DataOfBlock { get; private set; }

        public static DataBlock Read(Mdf mdf, ulong position)
        {
            mdf.UpdatePosition(position);

            var block = new DataBlock(mdf);
            block.Read();

            block.DataOfBlock = mdf.ReadBytes((int)block.Size - 24);

            return block;
        }
    }
}