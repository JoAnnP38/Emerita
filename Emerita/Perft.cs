using System.Collections.Concurrent;

namespace Emerita
{
    public sealed class Perft
    {
        public MoveList moveList = new();
        public Board board = new();
        public ConcurrentQueue<Move> moveQ;
        public static int NumTasks = 2;
        private readonly TtPerft ttPerftTable = new TtPerft(9000000);

        public Perft()
        {
            board.LoadNewGamePosition();
            moveQ = new ConcurrentQueue<Move>();
        }

        public void Initialize()
        {
            board.LoadNewGamePosition();
        }

        public ulong Execute(int depth)
        {
            if (depth == 0)
            {
                return 1ul;
            }

            ulong nodes = 0;

            board.GenerateMoves(moveList);
            Span<Move> moves = moveList.GetMoves(board.Ply);
            for (int i = 0; i < moves.Length; ++i)
            {
                if (board.MakeMove(moves[i]))
                {
                    nodes += Execute(depth - 1);
                    board.UnmakeMove();
                }
            }
            return nodes;
        }

        public ulong ExecuteMt(int depth)
        {
            if (depth == 0)
            {
                return 1ul;
            }

            ttPerftTable.Clear();
            board.GenerateMoves(moveList);
            moveQ = new(moveList.GetMoves(0).ToArray());

            Task<ulong>[] tasks = new Task<ulong>[NumTasks];
            for (int i = 0; i < NumTasks; ++i)
            {
                tasks[i] = Task<ulong>.Factory.StartNew(() => Execute1(board.Clone(), depth));
            }

            Task.WaitAll(tasks);

            ulong nodes = 0ul;
            for (int i = 0; i < NumTasks; ++i)
            {
                nodes += tasks[i].Result;
            }

            return nodes;
        }

        private ulong Execute1(Board bd, int depth)
        {
            ulong nodes = 0ul;
            MoveList mvList = new MoveList();

            while (!moveQ.IsEmpty)
            {
                if (moveQ.TryDequeue(out Move move))
                {
                    if (bd.MakeMove(move))
                    {
                        nodes += Execute2(bd, mvList, depth - 1);
                        bd.UnmakeMove();
                    }
                }
            }

            return nodes;
        }

        private ulong Execute2(Board bd, MoveList mvList, int depth)
        {
            if (depth == 0)
            {
                return 1ul;
            }

            ulong nodes = 0ul;

            bd.GenerateMoves(mvList);
            Span<Move> moves = mvList.GetMoves(bd.Ply);
            for (int i = 0; i < moves.Length; ++i)
            {
                if (bd.MakeMove(moves[i]))
                {
                    if (depth > 2)
                    {
                        ulong count = ttPerftTable.Lookup(bd.Hash, depth);
                        if (count > 0)
                        {
                            nodes += count;
                        }
                        else
                        {
                            count = Execute2(bd, mvList, depth - 1);
                            nodes += count;
                            ttPerftTable.Add(bd.Hash, depth, count);
                        }
                    }
                    else
                    {
                        nodes += Execute2(bd, mvList, depth - 1);
                    }
                    bd.UnmakeMove();
                }
            }

            return nodes;
        }


    }
}
