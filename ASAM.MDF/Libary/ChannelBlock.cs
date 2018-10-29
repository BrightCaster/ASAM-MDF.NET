namespace ASAM.MDF.Libary
{
    using System;
    using System.IO;
    using System.Text;

    using ASAM.MDF.Libary.Types;

    public class ChannelBlock : Block, INext<ChannelBlock>
    {
        private const int MIN_VERSION_LONG_SIGNAL_NAME = 212;
        private const int MIN_VERSION_DISPLAY_NAME = 300;
        private const int MIN_VERSION_ADDITIONAL_BYTE_OFFSET = 300;

        private uint ptrNextChannelBlock;
        private uint ptrChannelConversionBlock;
        private uint ptrChannelExtensionBlock;
        private uint ptrChannelDependencyBlock;
        private uint ptrChannelComment;
        private uint ptrLongSignalName;
        private uint ptrDisplayName;
        private string signalName;

        private ChannelConversionBlock channelConversion;
        private ChannelBlock next;
        private Stream stream;

        private ChannelBlock(Mdf mdf) : base(mdf)
        {
        }

        public ChannelBlock Next
        {
            get
            {
                if (next == null && ptrNextChannelBlock != 0)
                    next = Read(Mdf, stream, ptrNextChannelBlock);

                return next;
            }
        }
        public ChannelConversionBlock ChannelConversion
        {
            get
            {
                if (channelConversion == null && ptrChannelConversionBlock != 0)
                {
                    Mdf.Data.Position = ptrChannelConversionBlock;
                    channelConversion = new ChannelConversionBlock(Mdf);
                }

                return channelConversion;
            }
        }
        public ChannelExtensionBlock SourceDepending { get; private set; }
        public ChannelDependencyBlock Dependency { get; private set; }
        public TextBlock Comment { get; private set; }
        public ChannelType Type { get; private set; }
        public string SignalName
        {
            get { return signalName; }
            set { SetStringValue(ref signalName, value, 32); }
        }
        public string SignalDescription { get; private set; }
        public ushort BitOffset { get; private set; }
        public ushort NumberOfBits { get; private set; }
        public SignalType SignalType { get; private set; }
        public bool ValueRange { get; private set; }
        public double MinValue { get; private set; }
        public double MaxValue { get; private set; }
        public double SampleRate { get; private set; }
        public TextBlock LongSignalName { get; private set; }
        public TextBlock DisplayName { get; private set; }
        public ushort AdditionalByteOffset { get; private set; }

        public static ChannelBlock Create(Mdf mdf)
        {
            return new ChannelBlock(mdf)
            {
                Identifier = "CN",
                SignalName = "",
            };
        }
        public static ChannelBlock Read(Mdf mdf, Stream stream, uint position)
        {
            stream.Position = position;

            var block = new ChannelBlock(mdf);
            block.stream = stream;
            block.Read(stream);

            var data = new byte[block.Size - 4];
            var read = stream.Read(data, 0, data.Length);

            if (read != data.Length)
                throw new FormatException();

            block.next = null;
            block.SourceDepending = null;
            block.Dependency = null;
            block.Comment = null;

            block.ptrNextChannelBlock = BitConverter.ToUInt32(data, 0);
            block.ptrChannelConversionBlock = BitConverter.ToUInt32(data, 4);
            block.ptrChannelExtensionBlock = BitConverter.ToUInt32(data, 8);
            block.ptrChannelDependencyBlock = BitConverter.ToUInt32(data, 12);
            block.ptrChannelComment = BitConverter.ToUInt32(data, 16);
            block.Type = (ChannelType)BitConverter.ToUInt16(data, 20);
            block.SignalName = mdf.IDBlock.Encoding.GetString(data, 22, 32);
            block.SignalDescription = mdf.IDBlock.Encoding.GetString(data, 54, 128);
            block.BitOffset = BitConverter.ToUInt16(data, 182);
            block.NumberOfBits = BitConverter.ToUInt16(data, 184);
            block.SignalType = (SignalType)BitConverter.ToUInt16(data, 186);
            block.ValueRange = BitConverter.ToBoolean(data, 188);

            if (block.ValueRange)
            {
                block.MinValue = BitConverter.ToDouble(data, 190);
                block.MaxValue = BitConverter.ToDouble(data, 198);
            }

            block.SampleRate = BitConverter.ToDouble(data, 206);

            if (mdf.IDBlock.Version >= MIN_VERSION_LONG_SIGNAL_NAME)
                block.ptrLongSignalName = BitConverter.ToUInt32(data, 214);

            if (mdf.IDBlock.Version >= MIN_VERSION_DISPLAY_NAME)
                block.ptrDisplayName = BitConverter.ToUInt32(data, 218);

            if (mdf.IDBlock.Version >= MIN_VERSION_ADDITIONAL_BYTE_OFFSET)
                block.AdditionalByteOffset = BitConverter.ToUInt16(data, 222);

            if (block.ptrChannelExtensionBlock != 0)
            {
                stream.Position = block.ptrChannelExtensionBlock;
                block.SourceDepending = new ChannelExtensionBlock(mdf);
            }

            return block;
        }

        public override string ToString()
        {
            return SignalName;
        }

        internal override ushort GetSize()
        {
            // Base size.
            if (Mdf.IDBlock.Version < 212)
                return 218;

            // 2.12
            if (Mdf.IDBlock.Version < 300)
                return 222;

            // 3.00
            return 228;
        }
        internal override void Write(byte[] array, ref int index)
        {
            base.Write(array, ref index);

            var bytesSignalName = Mdf.IDBlock.Encoding.GetBytes(SignalName);

            Array.Copy(bytesSignalName, 0, array, index + 26, bytesSignalName.Length);

            index += GetSize();
        }
    }
}
