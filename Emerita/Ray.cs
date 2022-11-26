namespace Emerita
{
    public readonly struct Ray
    {
        public readonly ulong North;
        public readonly ulong NorthEast;
        public readonly ulong East;
        public readonly ulong SouthEast;
        public readonly ulong South;
        public readonly ulong SouthWest;
        public readonly ulong West;
        public readonly ulong NorthWest;

        public Ray(ulong north, ulong northEast, ulong east, ulong southEast, ulong south, ulong southWest, ulong west, ulong northWest)
        {
            North = north;
            NorthEast = northEast;
            East = east;
            SouthEast = southEast;
            South = south;
            SouthWest = southWest;
            West = west;
            NorthWest = northWest;
        }
    }
}
