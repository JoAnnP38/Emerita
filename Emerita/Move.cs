using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Emerita
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Move : IEquatable<Move>, IComparable<Move>
    {
        private sbyte piece;
        private sbyte from;
        private sbyte to;
        private sbyte flags;
        private sbyte capture;
        private sbyte promote;
        private short score;

        public Move()
        {
            piece = Constants.PIECE_NONE;
            from = Constants.D4;
            to = Constants.D4;
            flags = (sbyte)MoveFlags.NullMove;
            capture = Constants.PIECE_NONE;
            promote = Constants.PIECE_NONE;
            score = 0;
        }

        public Move(int piece, int from, int to, MoveFlags flags = MoveFlags.Normal, int capture = Constants.PIECE_NONE,
            int promote = Constants.PIECE_NONE, int score = 0)
        {
            this.piece = (sbyte)piece;
            this.from = (sbyte)from;
            this.to = (sbyte)to;
            this.flags = (sbyte)flags;
            this.capture = (sbyte)capture;
            this.promote = (sbyte)promote;
            this.score = (short)score;
        }

        public int Piece
        {
            get => piece;
            set => piece = (sbyte)value;
        }
        public int From
        {
            get => from;
            set => from = (sbyte)value;
        }

        public int To
        {
            get => to;
            set => to = (sbyte)value;
        }

        public MoveFlags Flags
        {
            get => (MoveFlags)flags; 
            set => flags = (sbyte)value;
        }

        public int Capture
        {
            get => capture; 
            set => capture = (sbyte)value;
        }

        public int Promote
        {
            get => promote; 
            set => promote = (sbyte)value;
        }

        public int Score
        {
            get => score; 
            set => score = (short)value;
        }

        public Move Copy(int newScore = -1)
        {
            return new Move(Piece, From, To, Flags, Capture, Promote, newScore == -1 ? Score : newScore);
        }

        public bool Equals(Move other)
        {
            return from == other.from &&
                   to == other.to &&
                   flags == other.flags &&
                   promote == other.promote &&
                   capture == other.capture &&
                   score == other.score;
        }

        public int CompareTo(Move other)
        {
            int compare = from.CompareTo(other.from);
            if (compare != 0)
            {
                return compare;
            }

            compare = to.CompareTo(other.to);
            if (compare != 0)
            {
                return compare;
            }

            compare = flags.CompareTo(other.flags);
            if (compare != 0)
            {
                return compare;
            }

            compare = capture.CompareTo(other.capture);
            if (compare != 0)
            {
                return compare;
            }

            compare = promote.CompareTo(other.promote);
            if (compare != 0)
            {
                return compare;
            }

            return score.CompareTo(other.score);
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
            return HashCode.Combine(from, to, flags, capture, promote, score);
        }

        public static Move NullMove { get; } = new();
    }
}
