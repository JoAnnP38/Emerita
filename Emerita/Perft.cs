using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emerita
{
    public sealed class Perft
    {
        public MoveList moveList = new();
        public Board board = new();

        public Perft()
        {
            board.LoadNewGamePosition();
        }

        public void Initialize()
        {
            board.LoadNewGamePosition();
        }

        public unsafe ulong Execute(int depth)
        {
            if (depth == 0)
            {
                return 1ul;
            }

            ulong nodes = 0;

            board.GenerateMoves(moveList);
            Span<Move> moves = moveList.GetMoves(board.Ply);
            fixed (Move* p = &moves[0])
            {
                for (int i = 0; i < moves.Length; ++i)
                {
                    if (board.MakeMove(p[i]))
                    {
                        nodes += Execute(depth - 1);
                        board.UnmakeMove();
                    }
                }
            }

            return nodes;
        }
    }
}
