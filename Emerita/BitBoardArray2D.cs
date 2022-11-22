using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Emerita
{
    public sealed class BitBoardArray2D
    {
        private readonly ulong[] array;
        private readonly int length1;
        private readonly int length2;

        public BitBoardArray2D(int length1, int length2)
        {
            array = new ulong[length1 * length2];
            this.length1 = length1;
            this.length2 = length2;
        }

        public bool IsFixedSize => true;
        public bool IsReadOnly => false;
        public int Length1 => length1;
        public int Length2 => length2;
        public int Length => array.Length;
        public int Rank => 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Array.Clear(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(ulong bb)
        {
            Array.Fill(array, bb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int i, int j)
        {
            return i * length2 + j;
        }

        public ref ulong this[int i, int j] => ref array[i * length2 + j];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetItem(int i, int j)
        {
            return array[GetIndex(i, j)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetItem(int i, int j, ulong value)
        {
            array[GetIndex(i, j)] = value;
        }

        public Span<ulong> this[int i] => new(array, i * length2, length2);
    }
}
