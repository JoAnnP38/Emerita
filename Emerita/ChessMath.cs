using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Emerita
{
    public static class ChessMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex(int index)
        {
            return (index & 0x03f) == index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidCoord(int coord)
        {
            return (coord & 0x07) == coord;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidColor(int color)
        {
            return (color & 0x01) == color;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsColorWhite(int color)
        {
            Util.Assert(IsValidColor(color));
            return (color & 0x01) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidPiece(int piece)
        {
            return piece >= 0 && piece < Constants.MAX_PIECES;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexToFile(int index)
        {
            Util.Assert(IsValidIndex(index));
            return index & 0x07;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexToRank(int index)
        {
            Util.Assert(IsValidIndex(index));
            return index >> 3;
        }

        public static void IndexToCoords(int index, out int file, out int rank)
        {
            Util.Assert(IsValidIndex(index), $"Invalid index passed to IndexToCoords: {index}");
            file = index & 0x07;
            rank = index >> 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CoordToIndex(int file, int rank)
        {
            Util.Assert(IsValidCoord(file));
            Util.Assert(IsValidCoord(rank));
            return (rank << 3) | file;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PolyglotPiece(int color, int piece)
        {
            Util.Assert(IsValidColor(color));
            Util.Assert(IsValidPiece(piece));
            return (piece << 1) | (color ^ 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetMask(int index)
        {
            Util.Assert(IsValidIndex(index));
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
        public static int LowestSetBitIndex(ulong bitBoard)
        {
#if X64
            return (int)Bmi1.X64.TrailingZeroCount(bitBoard);
#else
            if (bitBoard == 0ul)
            {
                return 64;
            }
            ulong bbi = bitBoard ^ (bitBoard - 1);
            uint folded = (uint)bbi ^ (uint)(bbi >> 32);
            return lsb64Table[folded * 0x78291ACF >> 26];
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ResetLowestSetBit(ulong bitBoard)
        {
#if X64
            return Bmi1.X64.ResetLowestSetBit(bitBoard);
#else
            return ResetBit(bitBoard, LowestSetBitIndex(bitBoard));
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
