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
        internal static void CopyAddress<T>(this T block, PointerAddress<uint> pointerData, byte[] bytes) where T : Block
        {
            var newbytes = BitConverter.GetBytes(pointerData.address);
            Array.Copy(newbytes, 0, bytes, block.BlockAddress + pointerData.offset, newbytes.Length);

        }
        internal static void CopyAddress<T>(this T block, PointerAddress<ulong> pointerData, byte[] bytes) where T : Block
        {
            var newbytes = BitConverter.GetBytes(pointerData.address);
            Array.Copy(newbytes, 0, bytes, block.BlockAddress + pointerData.offset, newbytes.Length);
        }
    }
}
