namespace Emerita
{
    public sealed class IntegerArray2D
    {
        private readonly int[] array;
        private readonly int length1;
        private readonly int length2;

        public IntegerArray2D(int length1, int length2)
        {
            array = new int[length1 * length2];
            this.length1 = length1;
            this.length2 = length2;
        }

        public bool IsFixedSize => true;
        public bool IsReadOnly => false;
        public int Length1 => length1;
        public int Length2 => length2;
        public int Length => array.Length;
        public int Rank => 2;

        public void Clear()
        {
            Array.Clear(array);
        }

        public void Fill(int fillValue)
        {
            Array.Fill(array, fillValue);
        }

        public int GetIndex(int i, int j)
        {
            return i * length2 + j;
        }

        public ref int this[int i, int j] => ref array[i * length2 + j];

        public int GetItem(int i, int j)
        {
            return array[GetIndex(i, j)];
        }

        public void SetItem(int i, int j, int value)
        {
            array[GetIndex(i, j)] = value;
        }

        public Span<int> this[int i] => new(array, i * length2, length2);

        public void Copy(IntegerArray2D other)
        {
            Util.Assert(length1 == other.length1 && length2 == other.length2);
            Array.Copy(other.array, array, array.Length);
        }

    }
}
