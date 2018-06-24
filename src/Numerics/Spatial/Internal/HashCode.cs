using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MathNet.Numerics.Random;

namespace MathNet.Numerics.Spatial.Internal
{
    // The contents of this file is taken from https://github.com/dotnet/coreclr/blob/master/src/mscorlib/shared/System/HashCode.cs
    // To be replaced by the framework implementation when released for the appropriate builds
#pragma warning disable SA1203, SA1101, SA1600, SA1512, SA1515, SA1503, SA1028, SA1308, SA1512, SA1202, SA1311, SA1028, SA1132, SA1309, SA1108, SA1520

    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
    // See the LICENSE file in the project root for more information.

    /*

    The xxHash32 implementation is based on the code published by Yann Collet:
    https://raw.githubusercontent.com/Cyan4973/xxHash/5c174cfa4e45a42f94082dc0d4539b39696afea1/xxhash.c

      xxHash - Fast Hash algorithm
      Copyright (C) 2012-2016, Yann Collet

      BSD 2-Clause License (http://www.opensource.org/licenses/bsd-license.php)

      Redistribution and use in source and binary forms, with or without
      modification, are permitted provided that the following conditions are
      met:

      * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
      * Redistributions in binary form must reproduce the above
      copyright notice, this list of conditions and the following disclaimer
      in the documentation and/or other materials provided with the
      distribution.

      THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
      "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
      LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
      A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
      OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
      SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
      LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
      DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
      THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
      (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
      OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

      You can contact the author at :
      - xxHash homepage: http://www.xxhash.com
      - xxHash source repository : https://github.com/Cyan4973/xxHash

    */
    // xxHash32 is used for the hash code.
    // https://github.com/Cyan4973/xxHash

    /// <summary>
    /// Generates a hashcode
    /// </summary>
    internal static class HashCode
    {
        private static readonly uint Seed = GenerateGlobalSeed();

        private const uint Prime1 = 2654435761U;
        private const uint Prime2 = 2246822519U;
        private const uint Prime3 = 3266489917U;
        private const uint Prime4 = 668265263U;
        private const uint Prime5 = 374761393U;

        private static uint GenerateGlobalSeed()
        {
            // NOTE: Modified from original unsafe implementation.
            int seed = RandomSeed.Robust();
            return unchecked((uint)seed);
        }

        public static int Combine<T1>(T1 value1)
        {
            // Provide a way of diffusing bits from something with a limited
            // input hash space. For example, many enums only have a few
            // possible hashes, only using the bottom few bits of the code. Some
            // collections are built on the assumption that hashes are spread
            // over a larger space, so diffusing the bits may help the
            // collection work more efficiently.

            var hc1 = (uint)(value1?.GetHashCode() ?? 0);

            uint hash = MixEmptyState();
            hash += 4;

            hash = QueueRound(hash, hc1);

            hash = MixFinal(hash);
            return (int)hash;
        }

        public static int CombineMany<T>(T[] items)
        {
            if (items == null)
            {
                return 0;
            }

            unchecked
            {
                int hashcode = 0;
                for (var i = 0; i < items.Length; i++)
                {
                    // HashCode.Combine(single) is partially diffuse so should be ok for this.
                    hashcode += Combine(items[i]);
                }

                return hashcode;
            }
        }

        public static int CombineMany<T>(List<T> items)
        {
            if (items == null)
            {
                return 0;
            }

            unchecked
            {
                int hashcode = 0;
                for (var i = 0; i < items.Count; i++)
                {
                    // HashCode.Combine(single) is partially diffuse so should be ok for this.
                    hashcode += Combine(items[i]);
                }

                return hashcode;
            }
        }

        public static int CombineMany<T>(IEnumerable<T> items)
        {
            if (items == null)
            {
                return 0;
            }

            unchecked
            {
                int hashcode = 0;
                foreach (var item in items)
                {
                    // HashCode.Combine(single) is partially diffuse so should be ok for this.
                    hashcode += Combine(item);
                }

                return hashcode;
            }
        }

        public static int Combine<T1, T2>(T1 value1, T2 value2)
        {
            var hc1 = (uint)(value1?.GetHashCode() ?? 0);
            var hc2 = (uint)(value2?.GetHashCode() ?? 0);

            uint hash = MixEmptyState();
            hash += 8;

            hash = QueueRound(hash, hc1);
            hash = QueueRound(hash, hc2);

            hash = MixFinal(hash);
            return (int)hash;
        }

        public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            var hc1 = (uint)(value1?.GetHashCode() ?? 0);
            var hc2 = (uint)(value2?.GetHashCode() ?? 0);
            var hc3 = (uint)(value3?.GetHashCode() ?? 0);

            uint hash = MixEmptyState();
            hash += 12;

            hash = QueueRound(hash, hc1);
            hash = QueueRound(hash, hc2);
            hash = QueueRound(hash, hc3);

            hash = MixFinal(hash);
            return (int)hash;
        }

        public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            var hc1 = (uint)(value1?.GetHashCode() ?? 0);
            var hc2 = (uint)(value2?.GetHashCode() ?? 0);
            var hc3 = (uint)(value3?.GetHashCode() ?? 0);
            var hc4 = (uint)(value4?.GetHashCode() ?? 0);

            Initialize(out uint v1, out uint v2, out uint v3, out uint v4);

            v1 = Round(v1, hc1);
            v2 = Round(v2, hc2);
            v3 = Round(v3, hc3);
            v4 = Round(v4, hc4);

            uint hash = MixState(v1, v2, v3, v4);
            hash += 16;

            hash = MixFinal(hash);
            return (int)hash;
        }

        public static int Combine<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
        {
            var hc1 = (uint)(value1?.GetHashCode() ?? 0);
            var hc2 = (uint)(value2?.GetHashCode() ?? 0);
            var hc3 = (uint)(value3?.GetHashCode() ?? 0);
            var hc4 = (uint)(value4?.GetHashCode() ?? 0);
            var hc5 = (uint)(value5?.GetHashCode() ?? 0);

            Initialize(out uint v1, out uint v2, out uint v3, out uint v4);

            v1 = Round(v1, hc1);
            v2 = Round(v2, hc2);
            v3 = Round(v3, hc3);
            v4 = Round(v4, hc4);

            uint hash = MixState(v1, v2, v3, v4);
            hash += 20;

            hash = QueueRound(hash, hc5);

            hash = MixFinal(hash);
            return (int)hash;
        }

        public static int Combine<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
        {
            var hc1 = (uint)(value1?.GetHashCode() ?? 0);
            var hc2 = (uint)(value2?.GetHashCode() ?? 0);
            var hc3 = (uint)(value3?.GetHashCode() ?? 0);
            var hc4 = (uint)(value4?.GetHashCode() ?? 0);
            var hc5 = (uint)(value5?.GetHashCode() ?? 0);
            var hc6 = (uint)(value6?.GetHashCode() ?? 0);

            Initialize(out uint v1, out uint v2, out uint v3, out uint v4);

            v1 = Round(v1, hc1);
            v2 = Round(v2, hc2);
            v3 = Round(v3, hc3);
            v4 = Round(v4, hc4);

            uint hash = MixState(v1, v2, v3, v4);
            hash += 24;

            hash = QueueRound(hash, hc5);
            hash = QueueRound(hash, hc6);

            hash = MixFinal(hash);
            return (int)hash;
        }

        public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
        {
            var hc1 = (uint)(value1?.GetHashCode() ?? 0);
            var hc2 = (uint)(value2?.GetHashCode() ?? 0);
            var hc3 = (uint)(value3?.GetHashCode() ?? 0);
            var hc4 = (uint)(value4?.GetHashCode() ?? 0);
            var hc5 = (uint)(value5?.GetHashCode() ?? 0);
            var hc6 = (uint)(value6?.GetHashCode() ?? 0);
            var hc7 = (uint)(value7?.GetHashCode() ?? 0);

            Initialize(out uint v1, out uint v2, out uint v3, out uint v4);

            v1 = Round(v1, hc1);
            v2 = Round(v2, hc2);
            v3 = Round(v3, hc3);
            v4 = Round(v4, hc4);

            uint hash = MixState(v1, v2, v3, v4);
            hash += 28;

            hash = QueueRound(hash, hc5);
            hash = QueueRound(hash, hc6);
            hash = QueueRound(hash, hc7);

            hash = MixFinal(hash);
            return (int)hash;
        }

        public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
        {
            var hc1 = (uint)(value1?.GetHashCode() ?? 0);
            var hc2 = (uint)(value2?.GetHashCode() ?? 0);
            var hc3 = (uint)(value3?.GetHashCode() ?? 0);
            var hc4 = (uint)(value4?.GetHashCode() ?? 0);
            var hc5 = (uint)(value5?.GetHashCode() ?? 0);
            var hc6 = (uint)(value6?.GetHashCode() ?? 0);
            var hc7 = (uint)(value7?.GetHashCode() ?? 0);
            var hc8 = (uint)(value8?.GetHashCode() ?? 0);

            Initialize(out uint v1, out uint v2, out uint v3, out uint v4);

            v1 = Round(v1, hc1);
            v2 = Round(v2, hc2);
            v3 = Round(v3, hc3);
            v4 = Round(v4, hc4);

            v1 = Round(v1, hc5);
            v2 = Round(v2, hc6);
            v3 = Round(v3, hc7);
            v4 = Round(v4, hc8);

            uint hash = MixState(v1, v2, v3, v4);
            hash += 32;

            hash = MixFinal(hash);
            return (int)hash;
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static uint Rol(uint value, int count)
            => (value << count) | (value >> (32 - count));

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static void Initialize(out uint v1, out uint v2, out uint v3, out uint v4)
        {
            v1 = Seed + Prime1 + Prime2;
            v2 = Seed + Prime2;
            v3 = Seed;
            v4 = Seed - Prime1;
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static uint Round(uint hash, uint input)
        {
            hash += input * Prime2;
            hash = Rol(hash, 13);
            hash *= Prime1;
            return hash;
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static uint QueueRound(uint hash, uint queuedValue)
        {
            hash += queuedValue * Prime3;
            return Rol(hash, 17) * Prime4;
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static uint MixState(uint v1, uint v2, uint v3, uint v4)
        {
            return Rol(v1, 1) + Rol(v2, 7) + Rol(v3, 12) + Rol(v4, 18);
        }

        private static uint MixEmptyState()
        {
            return Seed + Prime5;
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static uint MixFinal(uint hash)
        {
            hash ^= hash >> 15;
            hash *= Prime2;
            hash ^= hash >> 13;
            hash *= Prime3;
            hash ^= hash >> 16;
            return hash;
        }
    }
}
