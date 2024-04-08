namespace ASAM.MDF.Libary
{
    using System;
    using System.Text;

    /// <summary>
    /// The abstract block class
    /// </summary>
    public abstract class Block
    {
        protected Block(Mdf mdf)
        {
            if (mdf == null)
                throw new ArgumentNullException("mdf");

            Mdf = mdf;
        }

        public Mdf Mdf { get; private set; }
        [MdfVersion(400,0)]
        public ushort IdHash { get; private set; }
        public string Identifier { get; protected set; }
        [MdfVersion(400, 0)]
        public uint Reserved { get; set; }
        public ulong Size { get; protected set; }
        [MdfVersion(400, 0)]
        public ulong LinksCount { get; private set; }
        public ulong BlockAddress { get; private set; }

        internal virtual ushort GetSize()
        {
            return (ushort)Size;
        }
        internal virtual int GetSizeTotal()
        {
            return GetSize();
        }
        internal void Read()
        {
            BlockAddress = Mdf.position;
            if (Mdf.IDBlock.Version >= 400)
            {
                ReadV4();
                return;
            }
            Identifier = Mdf.GetString(2); // blockaddress = 0
            Size = Mdf.ReadU16();
        }
        internal virtual void Write(byte[] array, ref int index)
        {
            var bytesIdentifier = Encoding.UTF8.GetBytes(Identifier);
            var bytesSize = BitConverter.GetBytes(GetSize());

            Array.Copy(bytesIdentifier, 0, array, index, bytesIdentifier.Length);
            Array.Copy(bytesSize, 0, array, index + 2, bytesSize.Length);
        }

        /// <summary>
        /// Sets the string value.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">value</exception>
        protected void SetStringValue(ref string target, string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                target = value;
            }
            else
            {
                if (value.Length > maxLength)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                target = value;
            }
        }

        protected MdfVersionAttribute RequiredVersion(Type type, string property)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return (MdfVersionAttribute)Attribute.GetCustomAttribute(type.GetProperty(property), typeof(MdfVersionAttribute));
        }
        private void ReadV4()
        {
            IdHash = Mdf.ReadU16();
            Identifier = Mdf.GetString(2); // blockaddress = 0
            Reserved = Mdf.ReadU32();
            Size = Mdf.ReadU64();
            LinksCount = Mdf.ReadU64();
        }
    }
}
