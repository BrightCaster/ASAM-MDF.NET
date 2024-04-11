using System;

namespace ASAM.MDF.Libary
{
    internal static class Extensions
    {
        /// <summary>
        /// Copy address data to byte massive
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="block">Block coping</param>
        /// <param name="pointerData">pointer to change</param>
        /// <param name="bytes">byte massive for copy</param>
        internal static void CopyAddress<T>(this T block, PointerAddress pointerData, byte[] bytes) where T : Block
        {
            if (pointerData.version < 4)
            {
                var newbytes = BitConverter.GetBytes(pointerData.AddressV23);
                Array.Copy(newbytes, 0, bytes, block.BlockAddress + pointerData.offset, newbytes.Length);
            }
            else
            {
                var newbytes = BitConverter.GetBytes(pointerData.AddressV4);
                Array.Copy(newbytes, 0, bytes, block.BlockAddress + pointerData.offset, newbytes.Length);
            }
        }
    }
}
