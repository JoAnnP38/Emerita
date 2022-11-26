using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Emerita
{
    public static class BitOps
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetMask(int index)
        {
            Util.Assert(ChessMath.IsValidIndex(index));
            return 1ul << index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong SetBit(ulong bitBoard, int bitIndex)
        {
            return bitBoard | GetMask(bitIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ResetBit(ulong bitBoard, int bitIndex)
        {
#if X64
            return Bmi1.X64.AndNot(GetMask(bitIndex), bitBoard);
#else
            return bitBoard & ~GetMask(bitIndex);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBit(ulong bitBoard, int bitIndex)
        {
#if X64
            return (int)Bmi1.X64.BitFieldExtract(bitBoard, (byte)bitIndex, 1);
#else
            return bitBoard & GetMask(bitIndex) == 0ul ? 0 : 1;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TzCount(ulong bitBoard)
        {
#if X64
            return (int)Bmi1.X64.TrailingZeroCount(bitBoard);
#else
            return BitOperations.TrailingZeroCount(bitBoard);
            
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LzCount(ulong bitBoard)
        {
#if X64
            return (int)Lzcnt.X64.LeadingZeroCount(bitBoard);
#else
            return BitOperations.LeadingZeroCount(bitBoard);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ResetLsb(ulong bitBoard)
        {
#if X64
            return Bmi1.X64.ResetLowestSetBit(bitBoard);
#else
            return ResetBit(bitBoard, TrailingZeroCount(bitBoard));
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong AndNot(ulong bb1, ulong bb2)
        {
#if X64
            return Bmi1.X64.AndNot(bb2, bb1);
#else
            return bb1 & ~bb2;
#endif
        }

        public static int GreatestPowerOfTwoLessThan(int n)
        {
            int v = n;
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++; // next power of 2

            return v >> 1; // previous power of 2
        }

        private static readonly int[] lsb64Table =
        {
            63, 30,  3, 32, 59, 14, 11, 33,
            60, 24, 50,  9, 55, 19, 21, 34,
            61, 29,  2, 53, 51, 23, 41, 18,
            56, 28,  1, 43, 46, 27,  0, 35,
            62, 31, 58,  4,  5, 49, 54,  6,
            15, 52, 12, 40,  7, 42, 45, 16,
            25, 57, 48, 13, 10, 39,  8, 44,
            20, 47, 38, 22, 17, 37, 36, 26
        };
    }
}
