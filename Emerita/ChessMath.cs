using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

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

    }
}
