﻿namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;

    internal static class Common
    {
        internal static List<T> BuildBlockList<T>(List<T> list, T first) where T : INext<T>, IPrevious<T>
        {
            if (list == null)
            {
                list = new List<T>();
                T current = first;
                while (current != null)
                {
                    list.Add(current);

                    var prevCurrent = current;
                    current = current.Next;

                    if (current != null)
                        current.Previous = prevCurrent;
                }
            }

            return list;
        }

        internal static int GetSizeSafe(this Block block)
        {
            if (block == null)
                return 0;

            return block.GetSize();
        }

        internal static string Humanize(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return str.Replace("\0", "");
        }
    }
}