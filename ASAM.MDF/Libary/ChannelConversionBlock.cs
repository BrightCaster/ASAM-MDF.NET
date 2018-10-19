using System;

namespace ASAM.MDF.Libary
{
    using ASAM.MDF.Libary.Types;

    /// <summary>
    /// TODO: Incomplete
    /// </summary>
    public class ChannelConversionBlock// : Block
    {
        public bool PhysicalValueRangeValid { get; private set; }
        public double MinPhysicalValue { get; private set; }
        public double MaxPhysicalValue { get; private set; }
        public string PhysicalUnit { get; private set; }
        public ConversionType ConversionType { get; private set; }
        public UInt16 AdditionalConversionDataSizeInfo { get; private set; }
        // TODO: AdditionalConversionData
    }
}
