namespace ASAM.MDF.Libary
{
    using System;
    using System.IO;

    public class Mdf
    {
        /// <summary>
        /// Read MDF from stream.
        /// </summary>
        /// <param name="stream"></param>
        public Mdf(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (!stream.CanSeek)
                throw new ArgumentException("stream");

            Data = stream;
            Data.Position = 0;

            DataGroups = new DataGroupCollection(this);
            IDBlock = IdentificationBlock.Read(this, stream);
            HDBlock = HeaderBlock.Read(this, stream);
        }
        public Mdf()
        {
            DataGroups = new DataGroupCollection(this);
            IDBlock = IdentificationBlock.Create(this);
            HDBlock = HeaderBlock.Create(this);
        }

        public bool ReadOnly { get { return !Data.CanRead; } }

        public IdentificationBlock IDBlock { get; private set; }
        public HeaderBlock HDBlock { get; private set; }
        public DataGroupCollection DataGroups { get; private set; }

        internal Stream Data { get; private set; }

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
            size += HDBlock.GetSize();
            size += HDBlock.FileComment.GetSizeSafe();
            size += HDBlock.ProgramBlock.GetSizeSafe();

            for (int i = 0; i < DataGroups.Count; i++)
                size += DataGroups[i].GetSizeSafe();
            
            return size;
        }
    }
}
