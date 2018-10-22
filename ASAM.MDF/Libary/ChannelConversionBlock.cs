namespace ASAM.MDF.Libary
{
    using System;

    using ASAM.MDF.Libary.Types;

    public class ChannelConversionBlock : Block
    {
        public ChannelConversionBlock(Mdf mdf) : base(mdf)
        {
            var data = new byte[Size - 4];
            var read = Mdf.Data.Read(data, 0, data.Length);

            if (read != data.Length)
                throw new FormatException();

            PhysicalValueRangeValid = BitConverter.ToInt16(data, 0) != 0;
            MinPhysicalValue = BitConverter.ToDouble(data, 2);
            MaxPhysicalValue = BitConverter.ToDouble(data, 10);
            PhysicalUnit = Mdf.IDBlock.Encoding.GetString(data, 18, 20);
            ConversionType = (ConversionType)BitConverter.ToUInt16(data, 38);
        }

        public bool PhysicalValueRangeValid { get; private set; }
        public double MinPhysicalValue { get; private set; }
        public double MaxPhysicalValue { get; private set; }
        public string PhysicalUnit { get; private set; }
        public ConversionType ConversionType { get; private set; }
        public ushort AdditionalConversionDataSizeInfo { get; private set; }
        
        // TODO: AdditionalConversionData
    }
}
