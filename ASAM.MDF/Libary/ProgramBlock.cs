namespace ASAM.MDF.Libary
{
    using System;
    using System.Linq;

    public class ProgramBlock : Block
    {
        private byte[] data;

        private ProgramBlock(Mdf mdf) : base(mdf)
        {
        }

        public byte[] Data
        {
            get { return data; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                data = value;
            }
        }

        public static ProgramBlock Create(Mdf mdf)
        {
            return Create(mdf, new byte[0]);
        }
        public static ProgramBlock Create(Mdf mdf, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            return new ProgramBlock(mdf)
            {
                Identifier = "PR",
                Data = data
            };
        }

        public override string ToString()
        {
            return "{PRBLOCK: Data[" + Data.Length + "]}";
        }

        internal static ProgramBlock Read(Mdf mdf)
        {
            var block = new ProgramBlock(mdf);
            block.Read();

            block.Data = mdf.Data.Take(new Range(new Index((int)mdf.position),new Index((int)mdf.position + (int)block.Size))).ToArray();

            return block;
        }

        internal override ushort GetSize()
        {
            return (ushort)(4 + Data.Length);
        }
        internal override void Write(byte[] array, ref int index)
        {
            base.Write(array, ref index);

            Array.Copy(Data, 0, array, index + 4, Data.Length);

            index += GetSize();
        }
    }
}
