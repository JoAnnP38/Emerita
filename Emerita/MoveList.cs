using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Emerita
{
    public sealed class MoveList
    {
        private readonly Move[] moves = GC.AllocateUninitializedArray<Move>(Constants.MOVE_LIST_SIZE, true);
        private readonly int[] firstMove = new int[Constants.MAX_SEARCH_PLY];
        private int insertIndex = 0;
        private int currentPly = 0;

        public MoveList()
        {
            firstMove[0] = 0;
        }

        public void StartPly(int ply)
        {
            Util.Assert(ply < Constants.MAX_SEARCH_PLY - 1);
            insertIndex = firstMove[ply];
            firstMove[ply + 1] = insertIndex;
            currentPly = ply;
        }

        public void EndPly()
        {
            firstMove[currentPly + 1] = insertIndex;
        }

        public void Add(Move move)
        {
            Util.Assert(insertIndex < Constants.MOVE_LIST_SIZE);
            moves[insertIndex++] = move;
        }

        public void Add(int piece, int from, int to, MoveFlags flags = MoveFlags.Normal,
            int capture = Constants.PIECE_NONE,
            int promote = Constants.PIECE_NONE, int score = 0)
        {
            moves[insertIndex++] = new Move(piece, from, to, flags, capture, promote, score);
        }

        public Span<Move> GetMoves(int ply)
        {
            Util.Assert(ply < Constants.MAX_SEARCH_PLY - 1);
            return new Span<Move>(moves, firstMove[ply], firstMove[ply + 1] - firstMove[ply]);
        }

        public static unsafe Move GetNextMove(Span<Move> moves, int from)
        {
            fixed (Move* p = &moves[0])
            {
                int bestScore = p[from].Score;
                int bestIndex = from;

                for (int n = from + 1; n < moves.Length; ++n)
                {
                    if (p[n].Score > bestScore)
                    {
                        bestScore = p[n].Score;
                        bestIndex = n;
                    }
                }

                if (bestIndex != from)
                {
                    (p[from], p[bestIndex]) = (p[bestIndex], p[from]);
                }

                return p[from];
            }
        }
    }
}
