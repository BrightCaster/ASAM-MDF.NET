namespace ASAM.MDF.Libary
{
    internal class PointerAddress<T> where T : struct
    {
        internal readonly int offset;
        internal readonly T address;

        internal PointerAddress(T ptr, int offset)
        {
            this.address = ptr;
            this.offset = offset;
        }
    }
}
