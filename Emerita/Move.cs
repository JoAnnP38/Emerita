using System.Runtime.InteropServices;
using System.Text;

namespace Emerita
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct Move : IEquatable<Move>, IComparable<Move>
    {
        private readonly sbyte piece;
        private readonly sbyte from;
        private readonly sbyte to;
        private readonly byte flags;
        private readonly sbyte capture;
        private readonly sbyte promote;
        private readonly short score;

        public Move(int piece, int from, int to, MoveFlags flags = MoveFlags.Normal, int capture = Constants.PIECE_NONE,
            int promote = Constants.PIECE_NONE, int score = 0)
        {
            this.piece = (sbyte)piece;
            this.from = (sbyte)from;
            this.to = (sbyte)to;
            this.flags = (byte)flags;
            this.capture = (sbyte)capture;
            this.promote = (sbyte)promote;
            this.score = (short)score;
        }

        public int Piece => piece;
        public int From => from;
        public int To => to;
        public MoveFlags Flags => (MoveFlags)flags;
        public int Capture => capture;
        public int Promote => promote;
        public int Score => score;


        public Move Copy(int newScore = -1)
        {
            return new Move(Piece, From, To, Flags, Capture, Promote, newScore == -1 ? Score : newScore);
        }

        public bool Equals(Move other)
        {
            return Piece == other.Piece &&
                   From == other.From &&
                   To == other.To &&
                   Flags == other.Flags &&
                   Promote == other.Promote &&
                   Capture == other.Capture &&
                   Score == other.Score;
        }

        public int CompareTo(Move other)
        {
            int compare = Piece.CompareTo(other.Piece);
            if (compare != 0)
            {
                return compare;
            }

            compare = From.CompareTo(other.From);
            if (compare != 0)
            {
                return compare;
            }

            compare = To.CompareTo(other.To);
            if (compare != 0)
            {
                return compare;
            }

            compare = Flags.CompareTo(other.Flags);
            if (compare != 0)
            {
                return compare;
            }

            compare = Capture.CompareTo(other.Capture);
            if (compare != 0)
            {
                return compare;
            }

            compare = Promote.CompareTo(other.Promote);
            if (compare != 0)
            {
                return compare;
            }

            return Score.CompareTo(other.Score);
        }

        public string ToLongAlgebraic(bool useSeparator = false)
        {
            string separator = useSeparator ? "-" : string.Empty;
            string captureSeparator = useSeparator ? "x" : string.Empty;
            string promoteSeparator = useSeparator ? "=" : string.Empty;

            if (Flags == MoveFlags.NullMove)
            {
                return $"00{separator}00";
            }

            if (Flags == MoveFlags.Castle)
            {
                if (ChessMath.IndexToRank(To) == 2)
                {
                    return $"O{separator}O{separator}O";
                }

                return $"O{separator}O";
            }

            StringBuilder sb = new(32);
            if (Piece != Constants.PIECE_PAWN && Piece != Constants.PIECE_NONE)
            {
                sb.Append(ChessString.PieceToString(Piece));
            }
            sb.Append(ChessString.IndexToString(From));
            if (Flags is MoveFlags.Capture or MoveFlags.EnPassant or MoveFlags.PromoteCapture)
            {
                sb.Append(captureSeparator);
            }

            sb.Append(ChessString.IndexToString(To));
            if (Flags is MoveFlags.Promote or MoveFlags.PromoteCapture)
            {
                sb.Append(promoteSeparator);
                sb.Append(ChessString.PieceToString(Promote));
            }

            if (Flags == MoveFlags.EnPassant)
            {
                sb.Append("[ep]");
            }

            if (Flags == MoveFlags.Check)
            {
                sb.Append('+');
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new("(", 32);
            sb.Append(ChessString.PieceToString(Piece));
            sb.Append(',');
            sb.Append(ChessString.IndexToString(From));
            sb.Append(',');
            sb.Append(ChessString.IndexToString(To));
            sb.Append(',');
            sb.Append(Flags.ToString());
            sb.Append(',');
            sb.Append(ChessString.PieceToString(Capture));
            sb.Append(',');
            sb.Append(ChessString.PieceToString(Promote));
            sb.Append(',');
            sb.Append(Score);
            sb.Append(')');
            return sb.ToString();
        }

        public override bool Equals(object? obj)
        {
            return obj is Move move && Equals(move);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Piece, From, To, Flags, Capture, Promote, Score);
        }

        public static Move NullMove { get; } = 
            new(Constants.PIECE_NONE, Constants.D4, Constants.D4, MoveFlags.NullMove);
    }
}
