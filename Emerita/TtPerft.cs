namespace Emerita
{
    public class TtPerft
    {
        internal readonly struct TtPerftItem
        {
            internal readonly ulong Hash;
            internal readonly ulong Data;

            public TtPerftItem(ulong hash, uint depth, ulong count)
            {
                Data = (count << 4) | (depth & 0x00f);
                Hash = hash ^ Data;
            }

            public bool IsValid(ulong hash)
            {
                return hash == (Hash ^ Data);
            }

            internal uint Depth => (uint)(Data & 0x00f);
            internal ulong Count => Data >> 4;
        }

        internal readonly TtPerftItem[] Table;
        internal readonly uint Mask;

        public TtPerft(int capacity)
        {
            int c = BitOps.GreatestPowerOfTwoLessThan(capacity);
            Table = new TtPerftItem[c];
            Mask = (uint)(c - 1);
        }

        public ulong Lookup(ulong hash, int depth)
        {
            int index = (int)(hash & Mask);
            TtPerftItem item = Table[index];
            
            if (item.IsValid(hash) && item.Depth == (uint)depth)
            {
                return item.Count;
            }
            return 0;
        }

        public void Add(ulong hash, int depth, ulong count)
        {
            int index = (int)(hash & Mask);
            Table[index] = new TtPerftItem(hash, (uint)depth, count);
        }

        public void Clear()
        {
            Array.Clear(Table);
        }

        public int Capacity => Table.Length;
    }
}
