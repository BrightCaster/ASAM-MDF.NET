namespace ASAM.MDF.Libary
{
    using System;
    using System.IO;

    using ASAM.MDF.Libary.Types;

    public class ChannelConversionBlock : Block
    {
        private string physicalUnit;

        private ChannelConversionBlock(Mdf mdf) : base(mdf)
        {
        }

        public bool PhysicalValueRangeValid { get; set; }
        public double MinPhysicalValue { get; set; }
        public double MaxPhysicalValue { get; set; }
        public string PhysicalUnit
        {
            get { return physicalUnit; }
            set { SetStringValue(ref physicalUnit, value, 20); }
        }
        public ConversionType ConversionType { get; set; }
        public ushort SizeInformation { get; private set; }
        public byte[] AdditionalConversionData { get; set; }

        public static ChannelConversionBlock Create(Mdf mdf)
        {
            return new ChannelConversionBlock(mdf)
            {
                Identifier = "CC",
                PhysicalUnit = "",
            };
        }
        internal static ChannelConversionBlock Read(Mdf mdf, Stream stream, uint position)
        {
            stream.Position = position;

            var block = new ChannelConversionBlock(mdf);
            block.Read(stream);

            var data = new byte[block.Size - 4];
            var read = stream.Read(data, 0, data.Length);

            if (read != data.Length)
                throw new FormatException();

            block.PhysicalValueRangeValid = BitConverter.ToInt16(data, 0) != 0;
            block.MinPhysicalValue = BitConverter.ToDouble(data, 2);
            block.MaxPhysicalValue = BitConverter.ToDouble(data, 10);
            block.PhysicalUnit = mdf.IDBlock.Encoding.GetString(data, 18, 20).Humanize();
            block.ConversionType = (ConversionType)BitConverter.ToUInt16(data, 38);
            block.SizeInformation = BitConverter.ToUInt16(data, 40);

            if (block.SizeInformation > 0)
            {
                block.AdditionalConversionData = new byte[block.SizeInformation];

                Array.Copy(data, 42, block.AdditionalConversionData, 0, block.AdditionalConversionData.Length);
            }

            return block;
        }

        internal override ushort GetSize()
        {
            ushort size = 46;

            if (AdditionalConversionData != null)
                size += (ushort)AdditionalConversionData.Length;

            return size;
        }
        internal override void Write(byte[] array, ref int index)
        {
            base.Write(array, ref index);

            var bytesValueRange = BitConverter.GetBytes(PhysicalValueRangeValid);
            var bytesMinValue = BitConverter.GetBytes(MinPhysicalValue);
            var bytesMaxValue = BitConverter.GetBytes(MaxPhysicalValue);
            var bytesPhyUnit = Mdf.IDBlock.Encoding.GetBytes(PhysicalUnit);
            var bytesConversionType = BitConverter.GetBytes((ushort)ConversionType);
            
            Array.Copy(bytesValueRange, 0, array, index + 4, bytesValueRange.Length);
            Array.Copy(bytesMinValue, 0, array, index + 6, bytesMinValue.Length);
            Array.Copy(bytesMaxValue, 0, array, index + 14, bytesMaxValue.Length);
            Array.Copy(bytesPhyUnit, 0, array, index + 22, bytesPhyUnit.Length);
            Array.Copy(bytesConversionType, 0, array, index + 42, bytesConversionType.Length);

            if (AdditionalConversionData != null && AdditionalConversionData.Length > 0)
            {
                var bytesSizeInformation = BitConverter.GetBytes(AdditionalConversionData.Length);

                Array.Copy(bytesSizeInformation, 0, array, index + 44, bytesSizeInformation.Length);
                Array.Copy(AdditionalConversionData, 0, array, index + 46, AdditionalConversionData.Length);
            }

            index += GetSize();
        }
    }
}
