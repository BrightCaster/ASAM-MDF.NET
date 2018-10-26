namespace ASAM.MDF.Libary
{
    using System;
    using System.IO;

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

        internal static ProgramBlock Read(Mdf mdf, Stream stream)
        {
            var block = new ProgramBlock(mdf);
            block.Read(stream);

            if (block.Identifier != "PR")
                throw new FormatException();

            var data = new byte[block.Size - 4];
            var read = stream.Read(data, 0, data.Length);

            if (read != data.Length)
                throw new FormatException();

            block.data = data;

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
