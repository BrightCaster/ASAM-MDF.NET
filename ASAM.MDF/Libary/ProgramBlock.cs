namespace ASAM.MDF.Libary
{
    using System;

    public class ProgramBlock : Block
    {
        private byte[] m_Data;

        public ProgramBlock(Mdf mdf) : base(mdf)
        {
            if (Identifier != "PR")
                throw new FormatException();

            var data = new byte[Size - 4];
            var read = Mdf.Data.Read(data, 0, data.Length);

            if (read != data.Length)
                throw new FormatException();

            m_Data = data;
        }

        public byte[] Data
        {
            get
            {
                return m_Data;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_Data = value;
            }
        }
    }
}
