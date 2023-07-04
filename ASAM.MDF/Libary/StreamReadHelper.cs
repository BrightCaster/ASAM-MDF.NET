using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASAM.MDF.Libary
{
    public static class StreamReadHelper
    {
        public static byte ReadByte(this Mdf mdf)
        {
            return mdf.data[mdf.position++];
        }
        
        public static ushort ReadU16(this Mdf mdf)
        {
            var value = BitConverter.ToUInt16(mdf.data, (int)mdf.position);

            mdf.position += 2;

            return value;
        }
        public static uint ReadU32(this Mdf mdf)
        {
            var value = BitConverter.ToUInt32(mdf.data, (int)mdf.position);

            mdf.position += 4;

            return value;
        }
        public static ulong ReadU64(this Mdf mdf)
        {
            var value = BitConverter.ToUInt64(mdf.data, (int)mdf.position);

            mdf.position += 8;

            return value;
        }
        public static short Read16(this Mdf mdf)
        {
            var value = BitConverter.ToInt16(mdf.data, (int)mdf.position);

            mdf.position += 2;

            return value;
        }
        public static int Read32(this Mdf mdf)
        {
            var value = BitConverter.ToInt32(mdf.data, (int)mdf.position);

            mdf.position += 4;

            return value;
        }
        public static long Read64(this Mdf mdf)
        {
            var value = BitConverter.ToInt64(mdf.data, (int)mdf.position);

            mdf.position += 8;

            return value;
        }
        public static char ReadChar(this Mdf mdf)
        {
            var value = mdf.IDBlock.Encoding.GetString(mdf.data, (int)mdf.position, 2);

            mdf.position += 2;

            return value[0];
        }


        public static bool ReadBoolean(this Mdf mdf)
        {
            var value = BitConverter.ToUInt16(mdf.Data, (int)mdf.position);

            mdf.position += 2;

            return value != 0;
        }
        public static double ReadDouble(this Mdf mdf)
        {
            var value = BitConverter.ToDouble(mdf.Data, (int)mdf.position);

            mdf.position += 8;

            return value;
        }


        public static void UpdatePosition(this Mdf mdf, ulong address)
        {
            mdf.position = address;
        }
        public static int AdvanceIndex(this Mdf mdf, ulong count)
        {
            var index = mdf.position;

            mdf.position += count;

            return (int)index;
        }

        public static string GetString(this Mdf mdf, ulong count)
        {
            var value = mdf.IDBlock.Encoding.GetString(mdf.Data, mdf.AdvanceIndex(count), (int)count);
            var indexLastSymbol = value.IndexOf('\0');
            if (indexLastSymbol != -1)
                value = value.Remove(indexLastSymbol);
            
            return value;
        }
    }
}
