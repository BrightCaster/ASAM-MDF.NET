using System;
using System.Linq;
using System.Text;

namespace ASAM.MDF.Libary
{
    public class VectorCanBlockSupplement
    {
        public UInt32 IdentifierOfCanMessage;
        public UInt32 IndexOfCanChannel;
        public string NameOfMessage;
        public string NameOfSender;

        public VectorCanBlockSupplement(Mdf mdf)
        {
            byte[] data = new byte[80];
            data = mdf.Data.Take(data.Length).ToArray();

            IdentifierOfCanMessage = BitConverter.ToUInt32(data, 0);
            IndexOfCanChannel = BitConverter.ToUInt32(data, 4);
            NameOfMessage = Encoding.GetEncoding(mdf.IDBlock.CodePage).GetString(data, 8, 36);
            NameOfSender = Encoding.GetEncoding(mdf.IDBlock.CodePage).GetString(data, 44, 36);
        }

        internal VectorCanBlockSupplement Clone(Mdf mdf)
        {
            return MemberwiseClone() as VectorCanBlockSupplement;
        }
    }
}
