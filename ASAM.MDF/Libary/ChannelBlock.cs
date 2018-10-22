namespace ASAM.MDF.Libary
{
    using System;
    using System.Text;

    using ASAM.MDF.Libary.Types;

    public class ChannelBlock : Block, INext<ChannelBlock>
    {
        private uint m_ptrNextChannelBlock;
        private uint m_ptrChannelConversionBlock;
        private uint m_ptrChannelExtensionBlock;
        private uint m_ptrChannelDependencyBlock;
        private uint m_ptrChannelComment;
        private uint m_ptrLongSignalName;
        private uint m_ptrDisplayName;

        private ChannelConversionBlock channelConversion;

        private ChannelBlock m_Next;

        public ChannelBlock(Mdf mdf) : base(mdf)
        {
            var data = new byte[Size - 4];
            var read = Mdf.Data.Read(data, 0, data.Length);

            if (read != data.Length)
                throw new FormatException();

            m_Next = null;
            SourceDepending = null;
            Dependency = null;
            Comment = null;

            m_ptrNextChannelBlock = BitConverter.ToUInt32(data, 0);
            m_ptrChannelConversionBlock = BitConverter.ToUInt32(data, 4);
            m_ptrChannelExtensionBlock = BitConverter.ToUInt32(data, 8);
            m_ptrChannelDependencyBlock = BitConverter.ToUInt32(data, 12);
            m_ptrChannelComment = BitConverter.ToUInt32(data, 16);
            Type = (ChannelType)BitConverter.ToUInt16(data, 20);
            SignalName = Mdf.IDBlock.Encoding.GetString(data, 22, 32);
            SignalDescription = Mdf.IDBlock.Encoding.GetString(data, 54, 128);
            BitOffset = BitConverter.ToUInt16(data, 182);
            NumberOfBits = BitConverter.ToUInt16(data, 184);
            SignalType = (SignalType)BitConverter.ToUInt16(data, 186);
            ValueRange = BitConverter.ToBoolean(data, 188);
            if (ValueRange)
            {
                MinValue = BitConverter.ToDouble(data, 190);
                MaxValue = BitConverter.ToDouble(data, 198);
            }

            SampleRate = BitConverter.ToDouble(data, 206);

            var requiredVersionLongSignalName = RequiredVersion(typeof(ChannelBlock), "LongSignalName");
            if (requiredVersionLongSignalName != null && requiredVersionLongSignalName.Version <= Mdf.IDBlock.Version)
                m_ptrLongSignalName = BitConverter.ToUInt32(data, 214);

            var requiredVersionDisplayName = RequiredVersion(typeof(ChannelBlock), "DisplayName");
            if (requiredVersionDisplayName != null && requiredVersionDisplayName.Version <= Mdf.IDBlock.Version)
                m_ptrDisplayName = BitConverter.ToUInt32(data, 218);

            var requiredVersionAdditionalByteOffset = RequiredVersion(typeof(ChannelBlock), "AdditionalByteOffset");
            if (requiredVersionAdditionalByteOffset != null && requiredVersionAdditionalByteOffset.Version <= Mdf.IDBlock.Version)
                AdditionalByteOffset = BitConverter.ToUInt16(data, 222);

            if (m_ptrChannelExtensionBlock != 0)
            {
                mdf.Data.Position = m_ptrChannelExtensionBlock;
                SourceDepending = new ChannelExtensionBlock(mdf);
            }
        }

        public ChannelBlock Next
        {
            get
            {
                if (m_Next == null && m_ptrNextChannelBlock != 0)
                {
                    Mdf.Data.Position = m_ptrNextChannelBlock;
                    m_Next = new ChannelBlock(Mdf);
                }

                return m_Next;
            }
        }
        public ChannelConversionBlock ChannelConversion
        {
            get
            {
                if (channelConversion == null && m_ptrChannelConversionBlock != 0)
                {
                    Mdf.Data.Position = m_ptrChannelConversionBlock;
                    channelConversion = new ChannelConversionBlock(Mdf);
                }

                return channelConversion;
            }
        }
        public ChannelExtensionBlock SourceDepending { get; private set; }
        public ChannelDependencyBlock Dependency { get; private set; }
        public TextBlock Comment { get; private set; }
        public ChannelType Type { get; private set; }
        public string SignalName { get; private set; }
        public string SignalDescription { get; private set; }
        public ushort BitOffset { get; private set; }
        public ushort NumberOfBits { get; private set; }
        public SignalType SignalType { get; private set; }
        public bool ValueRange { get; private set; }
        public double MinValue { get; private set; }
        public double MaxValue { get; private set; }
        public double SampleRate { get; private set; }
        [MdfVersion(212, null)]
        public TextBlock LongSignalName { get; private set; }
        [MdfVersion(300, null)]
        public TextBlock DisplayName { get; private set; }
        [MdfVersion(300, 0)]
        public ushort AdditionalByteOffset { get; private set; }

        public override string ToString()
        {
            return SignalName;
        }
    }
}
