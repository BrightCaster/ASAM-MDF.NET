namespace ASAM.MDF.Libary
{
    using System;
    using System.Linq;
    using ASAM.MDF.Libary.Types;

    public class ConversionData
    {
        public ConversionData(ChannelConversionBlock parent)
        {
            Parent = parent;
        }

        public byte[] Data { get; set; }
        public ChannelConversionBlock Parent { get; private set; }

        public static ushort GetEstimatedParametersCount(ConversionType cType)
        {
            switch (cType)
            {
                case ConversionType.Linear:
                    return 2;

                case ConversionType.Exponential:
                case ConversionType.Logarithmic:
                    return 7;

                case ConversionType.RationalConversion:
                    return 6;
            }

            return 0;
        }
        public static ushort GetEstimatedParametersSize(ConversionType cType)
        {
            return (ushort)(GetEstimatedParametersCount(cType) * 8);
        }

        public double[] GetParameters()
        {
            if (Parent == null || Data == null)
                throw new FormatException();

            int estLength = 0;
            if (Parent.Mdf.IDBlock.Version >= 400)
                estLength = Parent.ValParamCount;
            else
                estLength = GetEstimatedParametersCount(Parent.ConversionType);


            if (Data.Length != estLength * 8)
                throw new FormatException();

            var values = new double[estLength];
            for (int i = 0; i < estLength; i++)
                values[i] = BitConverter.ToDouble(Data, i * 8);

            return values;
        }
        public double GetPhyValue(double intValue)
        {
            if (Parent == null || Data == null)
                return intValue;

            var p = GetParameters();
            if (Parent.Mdf.IDBlock.Version >= 400)
            {
                switch (Parent.ConversionType4)
                {
                    case ConversionType4.None: 
                        return intValue;

                    case ConversionType4.Linear:
                        return intValue * p[1] + p[0];

                    case ConversionType4.Rational:
                        return (p[0] * intValue * intValue + p[1] * intValue + p[2]) / (p[3] * intValue * intValue + p[4] * intValue + p[5]);

                    default:
                        throw new NotSupportedException("Conversion type '" + Parent.ConversionType4 + "' is not supported yet");
                }
            }
            else
            {
                switch (Parent.ConversionType)
                {
                    case ConversionType.Linear:
                        return intValue * p[1] + p[0];

                    case ConversionType.Exponential:
                        if (p[3] == 0)
                            return Math.Log10(((intValue - p[6]) * p[5] - p[2]) / p[0]) / p[1];

                        if (p[0] == 0)
                            return Math.Log10((p[2] / (intValue - p[6]) - p[5]) / p[3]) / p[4];

                        throw new NotSupportedException();

                    case ConversionType.Logarithmic:
                        if (p[3] == 0)
                            return Math.Exp(((intValue - p[6]) * p[5] - p[2]) / p[0]) / p[1];

                        if (p[0] == 0)
                            return Math.Exp((p[2] / (intValue - p[6]) - p[5]) / p[3]) / p[4];

                        throw new NotSupportedException();

                    case ConversionType.RationalConversion:
                        return (p[0] * intValue * intValue + p[1] * intValue + p[2]) / (p[3] * intValue * intValue + p[4] * intValue + p[5]);

                    default:
                        throw new NotSupportedException("Conversion type '" + Parent.ConversionType + "' is not supported yet");
                }
            }
        }
        public void SetParameters(params double[] parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            Data = new byte[parameters.Length * 8];

            for (int i = 0; i < parameters.Length; i++)
            {
                var pBytes = BitConverter.GetBytes(parameters[i]);

                Array.Copy(pBytes, 0, Data, i * 8, pBytes.Length);
            }
        }
        public object Clone(Mdf mdf)
        {
            var c = MemberwiseClone() as ConversionData;
            c.Parent = Parent.Clone(mdf) as ChannelConversionBlock;
            return c;
        }
    }
}
