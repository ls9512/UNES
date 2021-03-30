using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Aya.UNES
{
    public static class Utility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte AsByte(this bool to)
        {
            unsafe
            {
                var result = *((byte*) &to);
                return result;
            }
        }

        public static void Fill<T>(this T[] arr, T value)
        {
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
        }

        public static void Map<T>(this IEnumerable<T> enumerator, Action<T> go)
        {
            foreach (var e in enumerator)
            {
                go(e);
            }
        }
    }
}
