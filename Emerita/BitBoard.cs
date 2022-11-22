using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32.SafeHandles;

namespace Emerita
{
    public readonly struct BitBoard : IEquatable<BitBoard>, IComparable<BitBoard>
    {
        private readonly ulong bb = 0;

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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard(ulong bbValue)
        {
            bb = bbValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard SetBit(int index)
        {
            return new(bb | GetMask(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard ResetBit(int index)
        {
#if X64
            return new(Bmi1.X64.AndNot(bb, GetMask(index)));
#else
            return new BitBoard(bb & ~GetMask(index));
#endif
        }

        public int this[int i] => GetBit(i);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBit(int index)
        {
#if X64
            return (int)Bmi1.X64.BitFieldExtract(bb, (byte)index, 1);
#else
            return (bb & GetMask(index)) == 0ul ? 0 : 1;
#endif
        }

        public int LowestSetBitIndex()
        {
#if X64
            return (int)Bmi1.X64.TrailingZeroCount(bb);
#else
            if (bb == 0ul)
            {
                return 64;
            }
            ulong bbi = bb ^ (bb - 1);
            uint folded = (uint)bbi ^ (uint)(bbi >> 32);
            return lsb64Table[folded * 0x78291ACF >> 26];
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard ResetLowestSetBit()
        {
#if X64
            return new(Bmi1.X64.ResetLowestSetBit(bb));
#else
            return ResetBit(LowestSetBitIndex());
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard AndNot(BitBoard bitBoard)
        {
#if X64 
            return new(Bmi1.X64.AndNot(bitBoard.bb, bb));
#else
            return new(bb & ~bitBoard.bb);
#endif
        }

        public static BitBoard Empty { get; } = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator true(BitBoard bitBoard) => bitBoard.bb != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator false(BitBoard bitBoard) => bitBoard.bb == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !(BitBoard bitBoard) => bitBoard.bb == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BitBoard bb1, BitBoard bb2) => bb1.Equals(bb2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BitBoard bb1, BitBoard bb2) => !bb1.Equals(bb2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator ~(BitBoard bitBoard) => new(~bitBoard.bb);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator &(BitBoard bb1, BitBoard bb2) => new(bb1.bb & bb2.bb);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator ^(BitBoard bb1, BitBoard bb2) => new(bb1.bb ^ bb2.bb);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator <<(BitBoard bitBoard, int shift) => new(bitBoard.bb << shift);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator >> (BitBoard bitBoard, int shift) => new(bitBoard.bb >> shift);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator |(BitBoard bb1, BitBoard bb2) => new(bb1.bb | bb2.bb);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ulong(BitBoard bitBoard) => bitBoard.bb;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator BitBoard(ulong bbValue) => new(bbValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator bool(BitBoard bitBoard) => bitBoard.bb != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BitBoard other)
        {
            return bb == other.bb;
        }

        public int CompareTo(BitBoard other)
        {
            return bb < other.bb ? -1 : bb > other.bb ? 1 : 0;
        }

        public override bool Equals(object? obj)
        {
            return obj is BitBoard bitBoard && Equals(bitBoard);
        }

        public override int GetHashCode()
        {
            return bb.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            for (int j = 56; j >= 0; j -= 8)
            {
                for (int i = 0; i < 8; i++)
                {
                    sb.Append(GetBit(i + j));
                    sb.Append(' ');
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong GetMask(int index)
        {
            Util.Assert(index >= 0 && index < 64);
            return 1ul << index;
        }
    }
}
