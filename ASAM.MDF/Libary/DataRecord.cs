namespace Mdf
{
    using ASAM.MDF.Libary;

    public class DataRecord
    {
        public DataRecord(byte[] data)
        {
            this.Data = data;
        }

        public byte[] Data { get; private set; }
    }
}
