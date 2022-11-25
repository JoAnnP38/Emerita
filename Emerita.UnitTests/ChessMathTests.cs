using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Emerita.UnitTests
{
    [TestClass]
    public class ChessMathTests
    {
        public TestContext? TestContext { get; set; }

        [TestMethod]
        public void IsValidIndexTest()
        {
            Assert.IsFalse(ChessMath.IsValidIndex(-1));
            Assert.IsFalse(ChessMath.IsValidIndex(64));
            Assert.IsTrue(ChessMath.IsValidIndex(33));
        }

        [TestMethod]
        public void IsValidCoordTest()
        {
            Assert.IsFalse(ChessMath.IsValidCoord(-1));
            Assert.IsFalse(ChessMath.IsValidCoord(8));
            Assert.IsTrue(ChessMath.IsValidCoord(4));
        }

        [TestMethod]
        public void IsValidColorTest()
        {
            Assert.IsFalse(ChessMath.IsValidColor(-1));
            Assert.IsFalse(ChessMath.IsValidColor(2));
            Assert.IsTrue(ChessMath.IsValidColor(1));
        }

        [TestMethod]
        public void IsColorWhiteTest()
        {
            Assert.IsFalse(ChessMath.IsColorWhite(Constants.COLOR_BLACK));
            Assert.IsTrue(ChessMath.IsColorWhite(Constants.COLOR_WHITE));
        }

        [TestMethod]
        public void IsValidPieceTest()
        {
            Assert.IsFalse(ChessMath.IsValidPiece(-1));
            Assert.IsFalse(ChessMath.IsValidPiece(6));
            Assert.IsTrue(ChessMath.IsValidPiece(Constants.PIECE_KING));
        }

        [TestMethod]
        public void IndexToFileTest()
        {
            int file = ChessMath.IndexToFile(Constants.B3);
            Assert.AreEqual(1, file);
        }

        [TestMethod]
        public void IndexToRankTest()
        {
            int rank = ChessMath.IndexToRank(Constants.G5);
            Assert.AreEqual(4, rank);
        }

        [TestMethod]
        public void IndexToCoordsTest()
        {
            ChessMath.IndexToCoords(Constants.G5, out int file, out int rank);
            Assert.AreEqual(6, file);
            Assert.AreEqual(4, rank);
        }

        [TestMethod]
        public void CoordToIndexTest()
        {
            int index = ChessMath.CoordToIndex(6, 4);
            Assert.AreEqual(Constants.G5, index);
        }

        [TestMethod]
        public void IntrinsicTest()
        {
            int actual = ChessMath.TrailingZeroCount(0);
            Assert.AreEqual(64, actual);

            actual = ChessMath.LeadingZeroCount(0);
            Assert.AreEqual(64, actual);
        }
    }
}