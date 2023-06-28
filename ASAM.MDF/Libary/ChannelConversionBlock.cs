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
            AdditionalConversionData = new ConversionData(this);
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
        public byte Precision { get; private set; }
        public ushort Flags { get; private set; }
        public ushort SizeInformation { get; private set; }
        public ushort ValParamCount { get; private set; }
        public ConversionData AdditionalConversionData { get; internal set; }
        public ulong TextBlockName { get; private set; }
        public ulong TextBlockUnit { get; private set; }

        private ulong ptrFileComment;
        private int indexPointer;

        public ulong InverseConversion { get; private set; }
        public TextBlock FileComment { get; private set; }
        public TextBlock ConversionUnit { get; private set; }
        public TextBlock ConversionName { get; private set; }

        public static ChannelConversionBlock Create(Mdf mdf)
        {
            return new ChannelConversionBlock(mdf)
            {
                Identifier = "CC",
                PhysicalUnit = "",
            };
        }
        internal static ChannelConversionBlock Read(Mdf mdf, ulong position)
        {
            mdf.position = position;

            var block = new ChannelConversionBlock(mdf);
            block.Read();

            if (mdf.IDBlock.Version >= 400)
            {
                
                block.TextBlockName = mdf.ReadU64();
                block.TextBlockUnit = mdf.ReadU64();
                block.ptrFileComment = mdf.ReadU64();
                block.InverseConversion = mdf.ReadU64();
                
                if (block.LinksCount > 4)
                    mdf.UpdatePosition((block.LinksCount - 4) * 8);

                block.ConversionType = (ConversionType)mdf.ReadByte();
                block.Precision = mdf.ReadByte();
                block.Flags = mdf.ReadU16();
                block.SizeInformation = mdf.ReadU16();
                block.ValParamCount = mdf.ReadU16();
                block.MinPhysicalValue = mdf.ReadDouble();
                block.MaxPhysicalValue = mdf.ReadDouble();
                block.indexPointer = (int)mdf.position;

                block.AdditionalConversionData.Data = new byte[block.ValParamCount * 8];

                Array.Copy(mdf.Data, block.indexPointer, block.AdditionalConversionData.Data, 0, block.AdditionalConversionData.Data.Length);
            }
            else
            {
                block.PhysicalValueRangeValid = mdf.Read16() != 0;
                block.MinPhysicalValue = mdf.ReadDouble();
                block.MaxPhysicalValue = mdf.ReadDouble();
                block.PhysicalUnit = mdf.IDBlock.Encoding.GetString(mdf.Data, mdf.GetIndexator(20), 20).Humanize();
                block.ConversionType = (ConversionType)mdf.ReadU16();
                block.SizeInformation = mdf.ReadU16();

                if (block.SizeInformation > 0)
                {
                    block.AdditionalConversionData.Data = new byte[ConversionData.GetEstimatedParametersSize(block.ConversionType)];

                    Array.Copy(mdf.Data, (int)mdf.position, block.AdditionalConversionData.Data, 0, block.AdditionalConversionData.Data.Length);
                }
            }
            if (block.ptrFileComment != 0)
                block.FileComment = TextBlock.Read(mdf, block.ptrFileComment);
            
            if (block.TextBlockName != 0)
                block.ConversionName = TextBlock.Read(mdf, block.TextBlockName);
            
            if (block.TextBlockUnit != 0)
                block.ConversionUnit = TextBlock.Read(mdf, block.TextBlockUnit);

            return block;
        }

        internal override ushort GetSize()
        {
            ushort size = 46;

            if (AdditionalConversionData.Data != null)
                size += (ushort)AdditionalConversionData.Data.Length;

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

            if (AdditionalConversionData.Data != null && AdditionalConversionData.Data.Length > 0)
            {
                var bytesSizeInformation = BitConverter.GetBytes(ConversionData.GetEstimatedParametersCount(ConversionType));

                Array.Copy(bytesSizeInformation, 0, array, index + 44, bytesSizeInformation.Length);
                Array.Copy(AdditionalConversionData.Data, 0, array, index + 46, AdditionalConversionData.Data.Length);
            }

            index += GetSize();
        }
    }
}
