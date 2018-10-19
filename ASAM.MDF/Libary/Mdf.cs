namespace ASAM.MDF.Libary
{
    using System;
    using System.IO;

    public class Mdf
    {
        public Mdf(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (!stream.CanSeek)
                throw new ArgumentException("stream");

            Data = stream;
            Data.Position = 0;
            IDBlock = new IdentificationBlock(this);
            HDBlock = new HeaderBlock(this);
        }

        public bool ReadOnly { get { return !Data.CanRead; } }

        /// <summary>
        /// File identification.
        /// </summary>
        public IdentificationBlock IDBlock { get; private set; }

        /// <summary>
        /// File header.
        /// </summary>
        public HeaderBlock HDBlock { get; private set; }

        internal Stream Data { get; private set; }
        internal BlockCache Cache { get; private set; }
    }
}
