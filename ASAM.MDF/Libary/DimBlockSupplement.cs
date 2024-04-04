namespace ASAM.MDF.Libary
{
    using System;
    using System.Linq;
    using System.Text;

    public class DimBlockSupplement
    {
        public ushort NumberOfModule;
        public uint Address;
        public string Description;
        public string IdentificationOfEcu;

        public DimBlockSupplement(Mdf mdf)
        {
            byte[] data = new byte[118];
            data = mdf.Data.Take(data.Length).ToArray();

            NumberOfModule = BitConverter.ToUInt16(data, 0);
            Address = BitConverter.ToUInt32(data, 2);
            Description = Encoding.GetEncoding(mdf.IDBlock.CodePage).GetString(data, 6, 80);
            IdentificationOfEcu = Encoding.GetEncoding(mdf.IDBlock.CodePage).GetString(data, 86, 32);
        }
        public DimBlockSupplement Clone(Mdf mdf)
        {
            return MemberwiseClone() as DimBlockSupplement;
        }
    }
}
