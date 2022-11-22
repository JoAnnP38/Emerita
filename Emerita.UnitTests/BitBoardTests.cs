using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.Contracts;

namespace Emerita.UnitTests
{
    [TestClass]
    public class BitBoardTests
    {
        [TestMethod]
        public void DefaultCtorTest()
        {
            BitBoard bb = new();
            Assert.AreEqual(0ul, bb);
        }

        [TestMethod]
        public void CtorTest()
        {
            BitBoard bb = new(13ul);
            Assert.AreEqual(13ul, bb);
        }

        [TestMethod]
        public void SetBitTest()
        {
            BitBoard bb = new();
            bb = bb.SetBit(27);
            Assert.AreEqual(1, bb[27]);
        }

        [TestMethod]
        public void ResetBitTest()
        {
            BitBoard bb = new(0b0100ul);
            bb = bb.ResetBit(2);
            Assert.AreEqual(0ul, bb);
        }

        [TestMethod]
        public void IndexerTest()
        {
            BitBoard bb = new(0b010101ul);
            Assert.AreEqual(1, bb[4]);
            Assert.AreEqual(0, bb[3]);
            Assert.AreEqual(1, bb[2]);
            Assert.AreEqual(0, bb[1]);
            Assert.AreEqual(1, bb[0]);
        }

        [TestMethod]
        public void LowestSetBitIndexTest()
        {
            BitBoard bb = new(0b010010000ul);
            Assert.AreEqual(4, bb.LowestSetBitIndex());
        }

        [TestMethod]
        public void ResetLowestSetBitTest()
        {
            BitBoard bb = new(0b010010000ul);
            bb = bb.ResetLowestSetBit();
            Assert.AreEqual(0b010000000ul, bb);
        }

        [TestMethod]
        public void AndNotTest()
        {
            BitBoard bb = new(0b010010000ul);
            bb = bb.AndNot(new BitBoard(0b010000000ul));
            Assert.AreEqual(0b010000ul, bb);
        }

        [TestMethod]
        public void OperatorTrueFalseTest()
        {
            BitBoard bb = new(145ul);
            Assert.IsTrue(bb ? true : false);
            bb = new(0ul);
            Assert.IsFalse(bb ? true : false);
        }

        [TestMethod]
        public void OperatorLogicalNotTest()
        {
            BitBoard bb = new(0ul);
            Assert.IsTrue(!bb);
        }

        [TestMethod]
        public void OperatorEqualityTest()
        {
            BitBoard bb1 = new(123ul);
            BitBoard bb2 = new(234ul);
            BitBoard bb3 = new(123ul);

            Assert.IsFalse(bb1 == bb2);
            Assert.IsTrue(bb1 == bb3);
        }

        [TestMethod]
        public void OperatorInequalityTest()
        {
            BitBoard bb1 = new(123ul);
            BitBoard bb2 = new(234ul);
            BitBoard bb3 = new(123ul);

            Assert.IsTrue(bb1 != bb2);
            Assert.IsFalse(bb1 != bb3);
        }

        [TestMethod]
        public void OperatorBitwiseComplementTest()
        {
            BitBoard bb1 = new(1ul);
            BitBoard bb2 = ~bb1;
            Assert.AreEqual(0xFFFFFFFFFFFFFFFEul, bb2);
        }

        [TestMethod]
        public void OperatorBinaryAndTest()
        {
            BitBoard bb1 = new(0xFFul);
            BitBoard bb2 = new(0x03);
            BitBoard bb3 = bb1 & bb2;
            Assert.AreEqual(0x03ul, bb3);
        }

        [TestMethod]
        public void OperatorBinaryExclusiveOrTest()
        {
            BitBoard bb1 = new(0xFFul);
            BitBoard bb2 = new(0x03);
            BitBoard bb3 = bb1 ^ bb2;
            Assert.AreEqual(0xFCul, bb3);
            bb3 = bb3 ^ bb3;
            Assert.AreEqual(0ul, bb3);
        }

        [TestMethod]
        public void OperatorBinaryLeftShiftTest()
        {
            BitBoard bb1 = new(2ul);
            BitBoard bb2 = bb1 << 3;
            Assert.AreEqual(16ul, bb2);
        }

        [TestMethod]
        public void OperatorBinaryRightShiftTest()
        {
            BitBoard bb1 = new(157ul);
            BitBoard bb2 = bb1 >> 3;
            Assert.AreEqual(19ul, bb2);
        }

        [TestMethod]
        public void OperatorBinaryOrTest()
        {
            BitBoard bb1 = new(24ul);
            BitBoard bb2 = new(1ul);
            BitBoard bb3 = bb1 | bb2;
            Assert.AreEqual(25ul, bb3);
        }

        [TestMethod]
        public void OperatorExplicitBitBoardCastTest()
        {
            BitBoard bb = (BitBoard)123ul;
            Assert.AreEqual(123ul, bb);
        }

        [TestMethod]
        public void OperatorExplicitBoolCastTest()
        {
            BitBoard bb1 = new(37ul);
            BitBoard bb2 = new(0ul);

            Assert.IsTrue((bool)bb1);
            Assert.IsFalse((bool)bb2);
        }

        [TestMethod]
        public void CompareToTest()
        {
            BitBoard bb1 = new(5ul);
            BitBoard bb2 = new(7ul);
            BitBoard bb3 = new(5ul);

            Assert.IsTrue(bb1.CompareTo(bb2) < 0);
            Assert.IsTrue(bb2.CompareTo(bb1) > 0);
            Assert.IsTrue(bb1.CompareTo(bb3) == 0);
        }

        [TestMethod]
        public void ToStringTest()
        {
            BitBoard bb = new(157ul);
            string s = bb.ToString();
            string[] split = s.Split('\n');
            Assert.AreEqual(9, split.Length);
            Assert.AreEqual(string.Empty, split[8]);
        }
    }
}