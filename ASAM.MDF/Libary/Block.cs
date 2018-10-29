namespace ASAM.MDF.Libary
{
    using System;
    using System.IO;
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

        public ushort Size { get; private set; }
        public uint BlockAddress { get; private set; }
        public string Identifier { get; protected set; }

        internal virtual ushort GetSize()
        {
            return 0;
        }
        internal virtual int GetSizeTotal()
        {
            return GetSize();
        }
        internal void Read(Stream stream)
        {
            BlockAddress = (uint)stream.Position;

            var data = new byte[4];
            var read = stream.Read(data, 0, data.Length);

            if (read != data.Length)
                throw new FormatException();

            Identifier = Mdf.IDBlock.Encoding.GetString(data, 0, 2);
            Size = BitConverter.ToUInt16(data, 2);

            if (Size <= 4)
                throw new FormatException();
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
    }
}
