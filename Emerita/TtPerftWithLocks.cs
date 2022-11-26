namespace Emerita
{
    public class TtPerftWithLocks
    {
        internal readonly struct TtPerftItem
        {
            internal readonly ulong Hash = 0ul;
            internal readonly uint Depth = 0;
            internal readonly ulong Count = 0ul;

            public TtPerftItem()
            {
                Hash = 0ul;
                Depth = 0;
                Count = 0ul;
            }
            public TtPerftItem(ulong hash, uint depth, ulong count)
            {
                Hash = hash;
                Depth = depth;
                Count = count;
            }
        }

        internal readonly TtPerftItem[] Table;
        internal readonly uint Mask;
        internal readonly object[] LockObjects;

        public TtPerftWithLocks(int capacity)
        {
            int c = BitOps.GreatestPowerOfTwoLessThan(capacity);
            Table = new TtPerftItem[c];
            LockObjects = new object[c];
            Mask = (uint)(c - 1);
            for (int i = 0; i < c; i++)
            {
                LockObjects[i] = new object();
            }
        }

        public ulong Lookup(ulong hash, int depth)
        {
            int index = (int)(hash & Mask);
            lock (LockObjects[index])
            {
                TtPerftItem item = Table[index];

                if (item.Hash == hash && item.Depth == (uint)depth)
                {
                    return item.Count;
                }
            }
            return 0;
        }

        public void Add(ulong hash, int depth, ulong count)
        {
            int index = (int)(hash & Mask);
            lock (LockObjects[index])
            {
                Table[index] = new TtPerftItem(hash, (uint)depth, count);
            }
        }

        public void Clear()
        {
            Array.Clear(Table);
        }

        public int Capacity => Table.Length;
    }
}
