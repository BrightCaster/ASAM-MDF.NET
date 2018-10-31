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
        private string signalDescription;

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
                    channelConversion = ChannelConversionBlock.Read(Mdf, stream, ptrChannelConversionBlock);

                return channelConversion;
            }
            set { channelConversion = value; }
        }
        public ChannelExtensionBlock SourceDepending { get; private set; }
        public ChannelDependencyBlock Dependency { get; private set; }
        public TextBlock Comment { get; private set; }
        public ChannelType Type { get; set; }
        public string SignalName
        {
            get { return signalName; }
            set { SetStringValue(ref signalName, value, 32); }
        }
        public string SignalDescription
        {
            get { return signalDescription; }
            set { SetStringValue(ref signalDescription, value, 128); }
        }
        public ushort BitOffset { get; set; }
        public ushort NumberOfBits { get; set; }
        public SignalType SignalType { get; set; }
        public bool ValueRange { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double SampleRate { get; set; }
        public TextBlock LongSignalName { get; private set; }
        public TextBlock DisplayName { get; private set; }
        public ushort AdditionalByteOffset { get; set; }

        public static ChannelBlock Create(Mdf mdf)
        {
            return new ChannelBlock(mdf)
            {
                Identifier = "CN",
                SignalName = "",
                SignalDescription = "",
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
            block.SignalName = mdf.IDBlock.Encoding.GetString(data, 22, 32).Humanize();
            block.SignalDescription = mdf.IDBlock.Encoding.GetString(data, 54, 128).Humanize();
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
        internal override int GetSizeTotal()
        {
            var size = base.GetSizeTotal();

            if (channelConversion != null)
                size += channelConversion.GetSizeTotal();

            return size;
        }
        internal override void Write(byte[] array, ref int index)
        {
            base.Write(array, ref index);

            var bytesChannelType = BitConverter.GetBytes((ushort)Type);
            var bytesSignalName = Mdf.IDBlock.Encoding.GetBytes(SignalName);
            var bytesSignalDesc = Mdf.IDBlock.Encoding.GetBytes(SignalDescription);
            var bytesBitOffset = BitConverter.GetBytes(BitOffset);
            var bytesNumOfBits = BitConverter.GetBytes(NumberOfBits);
            var bytesSignalDataType = BitConverter.GetBytes((ushort)SignalType);
            var bytesValueRangeValid = BitConverter.GetBytes(ValueRange);
            var bytesMinValue = BitConverter.GetBytes(MinValue);
            var bytesMaxValue = BitConverter.GetBytes(MaxValue);
            var bytesSampleRate = BitConverter.GetBytes(SampleRate);

            Array.Copy(bytesChannelType, 0, array, index + 24, bytesChannelType.Length);
            Array.Copy(bytesSignalName, 0, array, index + 26, bytesSignalName.Length);
            Array.Copy(bytesSignalDesc, 0, array, index + 58, bytesSignalDesc.Length);
            Array.Copy(bytesBitOffset, 0, array, index + 186, bytesBitOffset.Length);
            Array.Copy(bytesNumOfBits, 0, array, index + 188, bytesNumOfBits.Length);
            Array.Copy(bytesSignalDataType, 0, array, index + 190, bytesSignalDataType.Length);
            Array.Copy(bytesValueRangeValid, 0, array, index + 192, bytesValueRangeValid.Length);
            Array.Copy(bytesMinValue, 0, array, index + 194, bytesMinValue.Length);
            Array.Copy(bytesMaxValue, 0, array, index + 202, bytesMaxValue.Length);
            Array.Copy(bytesSampleRate, 0, array, index + 210, bytesSampleRate.Length);

            if (Mdf.IDBlock.Version >= 212)
            {
                // TODO: LongSignalName.
            }

            if (Mdf.IDBlock.Version >= 300)
            {
                // TODO: DisplayName.
                var bytesAdditionalOffset = BitConverter.GetBytes(AdditionalByteOffset);

                Array.Copy(bytesAdditionalOffset, 0, array, index + 226, bytesAdditionalOffset.Length);
            }

            index += GetSize();
        }
        internal void WriteChannelConversion(byte[] array, ref int index, int blockIndex)
        {
            if (channelConversion == null)
                return;

            var bytesConversionIndex = BitConverter.GetBytes(index);

            Array.Copy(bytesConversionIndex, 0, array, blockIndex + 8, bytesConversionIndex.Length);

            ChannelConversion.Write(array, ref index);
        }
        internal void WriteNextChannelLink(byte[] array, int index, int blockIndex)
        {
            var bytesNextChannelLink = BitConverter.GetBytes(index);

            Array.Copy(bytesNextChannelLink, 0, array, blockIndex + 4, bytesNextChannelLink.Length);
        }
    }
}
