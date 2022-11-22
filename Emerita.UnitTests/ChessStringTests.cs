using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Emerita.UnitTests
{
    [TestClass]
    public class ChessStringTests
    {
        [TestMethod]
        public void IndexToStringTest()
        {
            string sq = ChessString.IndexToString(Constants.G5);
            Assert.AreEqual("g5", sq);
        }

        [TestMethod]
        public void FileToStringTest()
        {
            string fileString = ChessString.FileToString(3);
            Assert.AreEqual("d", fileString);
        }

        [TestMethod]
        public void RankToStringTest()
        {
            string rankString = ChessString.RankToString(3);
            Assert.AreEqual("4", rankString);
        }

        [TestMethod]
        public void PieceToStringTest()
        {
            string pieceString = ChessString.PieceToString(Constants.PIECE_KNIGHT);
            Assert.AreEqual("N", pieceString);
        }
    }
}