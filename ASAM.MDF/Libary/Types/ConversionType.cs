
using System.Collections.Generic;
using System.Windows.Documents;

namespace ASAM.MDF.Libary.Types
{
    public enum ConversionType : ushort
    {
        Linear = 0,
        TabularInterpolated,
        Tabular,
        Polynomial,
        Exponential,
        Logarithmic,
        RationalConversion,
        AsamMcd2Fomular,
        AsamMcd2Table,
        AsamMcd2RangeTable = 12,
        Date = 132,
        Time = 133,
        OneToOne = 65535,
    }
    public enum ConversionType4 : byte
    {
        None,
        Linear,
        Rational,
        Algebraic,
        TabularInterpolated,
        Tabular,
        RationalTabular,
        TabularToText,
        RationalTabularToText,
        TextToTabular,
        Translation,
        BitField
    }
    public class ConversionProp
    {
        public static void GetProp(ChannelConversionBlock block, Mdf mdf, ulong position, ulong nowPos)
        {
            mdf.UpdatePosition(position);

            switch (block.ConversionType4)
            {
                case ConversionType4.TabularToText:
                    {
                        block.ConvTabT = new List<TextBlock>();
                        var lastPosition = 0UL;
                        for (int i = 0; i < block.SizeInformation; i++)
                        {
                            lastPosition = mdf.position + 8;
                            var textProp = mdf.ReadU64();
                            if (textProp != 0)
                                block.ConvTabT.Add(TextBlock.Read(mdf, textProp));
                            mdf.UpdatePosition(lastPosition);
                        }
                        mdf.UpdatePosition(nowPos);
                    }
                    break;
            }
        }
        public static void GetValues(ChannelConversionBlock block, Mdf mdf)
        {
            switch (block.ConversionType4)
            {
                case ConversionType4.TabularToText:
                    {
                        block.ConvTabTValue = new List<double>();
                        for (int i = 0; i < block.ValParamCount; i++)
                        {
                            block.ConvTabTValue.Add(mdf.ReadDouble());
                        }
                    }
                    break;
            }
        }
    }
}
