namespace Emerita
{
    public sealed class Array2D<T> : ICloneable
    {
        private readonly T[] array;
        private readonly int length1;
        private readonly int length2;

        public Array2D(int length1, int length2)
        {
            array = new T[length1 * length2];
            this.length1 = length1;
            this.length2 = length2;
        }

        private Array2D(Array2D<T> array2d)
        {
            length1 = array2d.length1;
            length2 = array2d.length2;
            array = new T[length1 * length2];
            Array.Copy(array2d.array, array, array.Length);
        }

        public bool IsFixedSize => true;

        public bool IsReadOnly => false;

        public int Length1 => length1;

        public int Length2 => length2;

        public int Length => length1 * length2;

        public int Rank => 2;

        public void Clear()
        {
            Array.Clear(array);
        }

        public void Fill(T fillValue)
        {
            Array.Fill(array, fillValue);
        }

        public int GetIndex(int i, int j)
        {
            return i * length2 + j;
        }

        public ref T this[int i, int j] => ref array[i * length2 + j];

        public T GetItem(int i, int j)
        {
            return array[GetIndex(i, j)];
        }

        public void SetItem(int i, int j, T value)
        {
            array[GetIndex(i, j)] = value;
        }

        public Span<T> this[int i] => new(array, i * length2, length2);

        public Array2D<T> Clone()
        {
            return new Array2D<T>(this);
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
