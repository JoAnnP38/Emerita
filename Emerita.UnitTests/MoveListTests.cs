using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Emerita.UnitTests
{
    [TestClass]
    public class MoveListTests
    {
        [TestMethod]
        public void DefaultCtorTest()
        {
            MoveList moveList = new();
            Assert.IsNotNull(moveList);
        }

        [TestMethod]
        public void StartPlyTest()
        {
            try
            {
                MoveList moveList = new();
                moveList.StartPly(0);

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void EndPlyTest()
        {
            try
            {
                MoveList moveList = new();
                moveList.StartPly(0);
                moveList.Add(new Move(Constants.PIECE_PAWN, Constants.A2, Constants.A3, MoveFlags.PawnMove));
                moveList.Add(new Move(Constants.PIECE_PAWN, Constants.A2, Constants.A4, MoveFlags.DblPawnMove));
                moveList.EndPly();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void GetMovesTest()
        {
            MoveList moveList = new();
            moveList.StartPly(0);
            Move m1 = new(Constants.PIECE_PAWN, Constants.A2, Constants.A3, MoveFlags.PawnMove);
            Move m2 = new(Constants.PIECE_PAWN, Constants.A2, Constants.A4, MoveFlags.DblPawnMove);
            moveList.Add(m1);
            moveList.Add(m2);
            moveList.EndPly();
            Span<Move> moves = moveList.GetMoves(0);
            Assert.IsFalse(moves.IsEmpty);
            Assert.AreEqual(2, moves.Length);
            Assert.AreEqual(m1, moves[0]);
            Assert.AreEqual(m2, moves[1]);
        }

        [TestMethod]
        public void GetMovesTest2()
        {
            MoveList moveList = new();
            moveList.StartPly(0);
            Move m1 = new Move(Constants.PIECE_PAWN, Constants.A2, Constants.A3, MoveFlags.PawnMove);
            Move m2 = new Move(Constants.PIECE_PAWN, Constants.A2, Constants.A4, MoveFlags.DblPawnMove);
            moveList.Add(m1);
            moveList.Add(m2);
            moveList.EndPly();
            moveList.StartPly(1);
            Move m3 = new Move(Constants.PIECE_PAWN, Constants.H2, Constants.H3, MoveFlags.PawnMove);
            Move m4 = new Move(Constants.PIECE_PAWN, Constants.H2, Constants.H4, MoveFlags.DblPawnMove);
            moveList.Add(m3);
            moveList.Add(m4);
            moveList.EndPly();
            Span<Move> moves = moveList.GetMoves(1);
            Assert.IsFalse(moves.IsEmpty);
            Assert.AreEqual(2, moves.Length);
            Assert.AreEqual(m3, moves[0]);
            Assert.AreEqual(m4, moves[1]);
        }

        [TestMethod]
        public void GetNextMoveTest()
        {
            MoveList moveList = new();
            moveList.StartPly(0);
            Move m1 = new Move(Constants.PIECE_PAWN, Constants.A2, Constants.A3, MoveFlags.PawnMove, score: 1);
            Move m2 = new Move(Constants.PIECE_PAWN, Constants.A2, Constants.A4, MoveFlags.DblPawnMove, score: 2);
            Move m3 = new Move(Constants.PIECE_PAWN, Constants.H2, Constants.H3, MoveFlags.PawnMove, score: 3);
            Move m4 = new Move(Constants.PIECE_PAWN, Constants.H2, Constants.H4, MoveFlags.DblPawnMove, score: 4);
            moveList.Add(m3);
            moveList.Add(m2);
            moveList.Add(m4);
            moveList.Add(m1);
            moveList.EndPly();

            Span<Move> moves = moveList.GetMoves(0);
            Move s1 = MoveList.GetNextMove(moves, 0);
            Move s2 = MoveList.GetNextMove(moves, 1);
            Move s3 = MoveList.GetNextMove(moves, 2);
            Move s4 = MoveList.GetNextMove(moves, 3);

            Assert.AreEqual(m4, s1);
            Assert.AreEqual(m3, s2);
            Assert.AreEqual(m2, s3);
            Assert.AreEqual(m1, s4);
        }
    }
}