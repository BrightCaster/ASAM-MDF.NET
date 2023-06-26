namespace ASAM.MDF.Libary
{
    using System;
    using System.IO;

    public class Mdf
    {
        internal ulong position;
        internal byte[] data;

        /// <summary>
        /// Read MDF from stream.
        /// </summary>
        /// <param name="stream"></param>
        public Mdf(byte[] bytes)
        {
            data = bytes;

            DataGroups = new DataGroupCollection(this);
            IDBlock = IdentificationBlock.Read(this);
            HDBlock = HeaderBlock.Read(this);
        }
        public Mdf()
        {
            DataGroups = new DataGroupCollection(this);
            IDBlock = IdentificationBlock.Create(this);
            HDBlock = HeaderBlock.Create(this);
        }


        public IdentificationBlock IDBlock { get; private set; }
        public HeaderBlock HDBlock { get; private set; }
        public DataGroupCollection DataGroups { get; private set; }

        internal byte[] Data => data;

        public byte[] GetBytes()
        {
            var array = new byte[GetSize()];

            int index = 0;

            // IDBLOCK.
            IDBlock.Write(array, ref index);

            int hdBlockIndex = index;

            // HDBLOCK.
            HDBlock.Write(array, ref index);
            HDBlock.WriteFileComment(array, ref index, hdBlockIndex);
            HDBlock.WriteProgramBlock(array, ref index, hdBlockIndex);
            HDBlock.WriteFirstDataGroupLink(array, index, hdBlockIndex);

            // DGBLOCKs.
            DataGroups.Write(array, ref index);
            
            return array;
        }

        internal int GetSize()
        {
            var size = 0;

            size += IDBlock.GetSize();
            size += HDBlock.GetSizeTotal();

            for (int i = 0; i < DataGroups.Count; i++)
                size += DataGroups[i].GetSizeTotal();

            return size;
        }

        internal byte[] ReadBytes(ushort recordSize)
        {
            var c = 0;
            if ((int)position + recordSize >= data.Length)
                c++;

            var value = new byte[recordSize];

            Array.Copy(data, (int)position, value, 0, value.Length);

            position += (ulong)value.Length;

            return value;
        }
        internal string GetNameBlock(ulong position)
        {
            var index = position + 2;
            var name = IDBlock.Encoding.GetString(Data, (int)index, 2);
            return name;
        }
    }
}
