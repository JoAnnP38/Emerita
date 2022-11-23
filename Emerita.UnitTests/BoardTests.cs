using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace Emerita.UnitTests
{
    [TestClass]
    public class BoardTests
    {
        [TestMethod]
        public void DefaultCtorTest()
        {
            Board bd = new();
            Assert.IsNotNull(bd);
            Assert.IsFalse(bd.GameBoard.IsEmpty);
            Assert.AreEqual(Constants.MAX_SQUARES, bd.GameBoard.Length);
            Assert.IsTrue(bd.GameBoard.ToArray().All(sq => sq == Constants.PIECE_NONE));
            Assert.AreEqual(0ul, bd.Units(0) | bd.Units(1));
            Assert.AreEqual(0ul, bd.All);
            Assert.AreEqual(0, bd.Ply);
            Assert.AreEqual(0, bd.HalfMoveClock);
            Assert.AreEqual(0, bd.FullMoveCounter);
            Assert.AreEqual(CastlingFlags.None, bd.Castling);
            Assert.IsNull(bd.EnPassant);
            Assert.AreEqual(0, bd.SideToMove);
            Assert.AreEqual(0ul, bd.Hash);
            Assert.AreEqual(0ul, bd.PawnHash);
        }

        [TestMethod]
        public void AddPieceTest()
        {
            Board bd = new();
            bd.AddPiece(Constants.COLOR_WHITE, Constants.PIECE_ROOK, Constants.A1);
            Assert.AreEqual(Constants.PIECE_ROOK, bd.GameBoard[Constants.A1]);
            Assert.AreEqual(1ul, bd.Pieces(Constants.COLOR_WHITE, Constants.PIECE_ROOK));
            Assert.AreEqual(1ul, bd.Units(Constants.COLOR_WHITE));
            Assert.AreEqual(0ul, bd.Units(Constants.COLOR_BLACK));
            Assert.AreEqual(1ul, bd.All);
            Assert.AreNotEqual(0ul, bd.Hash);
            Assert.AreEqual(0ul, bd.PawnHash);
        }

        [TestMethod]
        public void RemovePieceTest()
        {
            Board bd = new();
            bd.AddPiece(Constants.COLOR_WHITE, Constants.PIECE_ROOK, Constants.A1);
            bd.RemovePiece(Constants.COLOR_WHITE, Constants.PIECE_ROOK, Constants.A1);
            Assert.AreEqual(Constants.PIECE_NONE, bd.GameBoard[Constants.A1]);
            Assert.AreEqual(0ul, bd.Pieces(Constants.COLOR_WHITE, Constants.PIECE_ROOK));
            Assert.AreEqual(0ul, bd.Units(Constants.COLOR_WHITE));
            Assert.AreEqual(0ul, bd.Units(Constants.COLOR_BLACK));
            Assert.AreEqual(0ul, bd.All);
            Assert.AreEqual(0ul, bd.Hash);
            Assert.AreEqual(0ul, bd.PawnHash);
        }

        [TestMethod]
        [DataRow(0, 0, new[] { Constants.A2, Constants.B2, Constants.C2, Constants.D2, Constants.E2, Constants.F2, Constants.G2, Constants.H2 })]
        [DataRow(1, 1, new[] { Constants.B8, Constants.G8 })]
        [DataRow(0, 2, new[] { Constants.C1, Constants.F1 })]
        [DataRow(1, 3, new[] { Constants.A8, Constants.H8 })]
        [DataRow(0, 4, new[] { Constants.D1 })]
        [DataRow(1, 5, new[] { Constants.E8 })]
        public void LoadNewGamePositionTest(int color, int piece, IEnumerable<int> squares)
        {
            Board bd = new();
            bd.LoadNewGamePosition();
            List<int> locations = new(squares);
            ulong bb = bd.Pieces(color, piece);
            List<int> locs = new();

            while (bb != 0)
            {
                locs.Add(ChessMath.LowestSetBitIndex(bb));
                ChessMath.ResetLowestSetBit(ref bb);
            }

            bool allExpectedInSet = locations.All(sq => locs.Contains(sq));
            bool noneInSetUnexpected = locs.All(sq => locations.Contains(sq));

            Assert.IsTrue(allExpectedInSet);
            Assert.IsTrue(noneInSetUnexpected);
        }

        [TestMethod]
        [DataRow("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 0x463b96181691fc9cUL)]
        [DataRow("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", 0x823c9b50fd114196UL)]
        [DataRow("rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2", 0x0756b94461c50fb0UL)]
        [DataRow("rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR b KQkq - 0 2", 0x662fafb965db29d4UL)]
        [DataRow("rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPP1PPP/RNBQKBNR w KQkq f6 0 3", 0x22a48b5a8e47ff78UL)]
        [DataRow("rnbq1bnr/ppp1pkpp/8/3pPp2/8/8/PPPPKPPP/RNBQ1BNR w - - 0 4", 0x00fdd303c946bdd9UL)]
        [DataRow("rnbqkbnr/p1pppppp/8/8/PpP4P/8/1P1PPPP1/RNBQKBNR b KQkq c3 0 3", 0x3c8123ea7b067637UL)]
        [DataRow("rnbqkbnr/p1pppppp/8/8/P6P/R1p5/1P1PPPP1/1NBQKBNR b Kkq - 0 4", 0x5c3f9b829b279560UL)]
        [DataRow("rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPPKPPP/RNBQ1BNR b kq - 0 3", 0x652a607ca3f242c1UL)]
        public void LoadFenPositionTest(string fen, ulong expectedHash)
        {
            Board bd = new();
            bd.LoadFenPosition(fen);
            Assert.AreEqual(expectedHash, bd.Hash);
        }

        [TestMethod]
        [DataRow("rnb1k2r/pp3ppp/3bpq2/2p5/2B2P2/1P3Q2/P1PPN1PP/2B2RK1 w kq - 2 11")]
        [DataRow("rnbqkbnr/p1pppppp/8/8/PpP4P/8/1P1PPPP1/RNBQKBNR b KQkq c3 0 3")]
        public void ToFenStringTest(string expectedFen)
        {
            Board bd = new();
            bd.LoadFenPosition(expectedFen);
            Assert.AreEqual(expectedFen, bd.ToFenString());
        }

        [TestMethod]
        public void GenerateMovesTest()
        {
            Board bd = new();
            bd.LoadNewGamePosition();
            MoveList moveList = new();
            bd.GenerateMoves(moveList);
            Span<Move> moves = moveList.GetMoves(0);
            Assert.AreEqual(20, moves.Length);
        }

        [TestMethod]
        public void GenerateMovesTest2()
        {
            Board bd = new();
            bd.LoadFenPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1");
            MoveList moveList = new();
            bd.GenerateMoves(moveList);
            Span<Move> moves = moveList.GetMoves(0);
            Assert.AreEqual(20, moves.Length);
        }

        [TestMethod]
        [DataRow("rnbqkbnr/p1pppppp/8/8/P6P/R1p5/1P1PPPP1/1NBQKBNR b Kkq - 0 4", 23)]
        [DataRow("r2qk2r/pb4pp/1n2Pb2/2B2Q2/p1p5/2P5/2B2PPP/RN2R1K1 w - - 1 0", 47)]
        [DataRow("r2qk2r/pb4pp/1n2Pb2/2B2Q2/p1p5/2P5/2B2PPP/RN2R1K1 b - - 1 0", 42)]
        public void GenerateMovesTest3(string fen, int expectedMoveCount)
        {
            Board bd = new();
            bd.LoadFenPosition(fen);
            MoveList moveList = new();
            bd.GenerateMoves(moveList);
            Span<Move> moves = moveList.GetMoves(0);
            Assert.AreEqual(expectedMoveCount, moves.Length);
        }

        [TestMethod]
        [DataRow("r2qk2r/pb4pp/1n2Pb2/2B2Q2/p1p5/2P5/2B2PPP/RN2R1K1 w - - 1 0")]
        [DataRow("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")]
        [DataRow("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1")]
        [DataRow("rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2")]
        [DataRow("rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR b KQkq - 0 2")]
        [DataRow("rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPP1PPP/RNBQKBNR w KQkq f6 0 3")]
        [DataRow("rnbq1bnr/ppp1pkpp/8/3pPp2/8/8/PPPPKPPP/RNBQ1BNR w - - 0 4")]
        [DataRow("rnbqkbnr/p1pppppp/8/8/PpP4P/8/1P1PPPP1/RNBQKBNR b KQkq c3 0 3")]
        [DataRow("rnbqkbnr/p1pppppp/8/8/P6P/R1p5/1P1PPPP1/1NBQKBNR b Kkq - 0 4")]
        [DataRow("rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPPKPPP/RNBQ1BNR b kq - 0 3")]
        public void MakeMoveTest(string fen)
        {
            Board bd = new();
            bd.LoadFenPosition(fen);
            MoveList moveList = new();
            bd.GenerateMoves(moveList);
            Span<Move> moves = moveList.GetMoves(0);
            Move[] expectedMoves = moves.ToArray();

            for (int i = 0; i < expectedMoves.Length; ++i)
            {
                if (bd.MakeMove(expectedMoves[i]))
                {
                    bd.UnmakeMove();
                }
                bd.GenerateMoves(moveList);
                Move[] actualMoves = moveList.GetMoves(0).ToArray();
                Assert.AreEqual(expectedMoves.Length, actualMoves.Length, $"Move arrays differ after move: {expectedMoves[i]}");
                for (int n = 0; n < expectedMoves.Length; ++n)
                {
                    Assert.AreEqual(expectedMoves[n], actualMoves[n], $"Move arrays differ after move: {expectedMoves[i]}");
                }

            }
        }
    }
}