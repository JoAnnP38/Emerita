using System.Runtime.InteropServices;

namespace Emerita.UnitTests
{
    [TestClass]
    public class Array2DTests
    {
        [TestMethod]
        public void CtorTest()
        {
            Array2D<int> array = new(2, 3);
            Assert.IsTrue(array.IsFixedSize);
            Assert.IsFalse(array.IsReadOnly);
            Assert.AreEqual(2, array.Length1);
            Assert.AreEqual(3, array.Length2);
            Assert.AreEqual(6, array.Length);
            Assert.AreEqual(2, array.Rank);
        }

        [TestMethod]
        public void ClearTest()
        {
            Array2D<int> array = new(2, 3);
            array[0, 0] = 1;
            array[0, 1] = 2;
            array[0, 2] = 3;
            array[1, 0] = 4;
            array[1, 1] = 5;
            array[1, 2] = 6;

            array.Clear();
            for (int x = 0; x < 2; ++x)
                for (int y = 0; y < 3; ++y)
                    Assert.AreEqual(0, array[x, y]);
        }

        [TestMethod]
        public void FillTest()
        {
            Array2D<int> array = new(2, 3);
            array.Fill(3);

            for (int x = 0; x < 2; ++x)
                for (int y = 0; y < 3; ++y)
                    Assert.AreEqual(3, array[x, y]);
        }

        [TestMethod]
        public void SpanTest()
        {
            Array2D<int> array = new(2, 3);
            array[0, 0] = 1;
            array[0, 1] = 2;
            array[0, 2] = 3;
            array[1, 0] = 4;
            array[1, 1] = 5;
            array[1, 2] = 6;

            Span<int> span = array[0];

            Assert.AreEqual(3, span.Length);
            Assert.AreEqual(1, span[0]);
            Assert.AreEqual(2, span[1]);
            Assert.AreEqual(3, span[2]);
        }

        [TestMethod]
        public void CloneTest()
        {
            Array2D<int> array = new(2, 3);
            array[0, 0] = 1;
            array[0, 1] = 2;
            array[0, 2] = 3;
            array[1, 0] = 4;
            array[1, 1] = 5;
            array[1, 2] = 6;

            Array2D<int> clone = array.Clone();

            Assert.AreNotSame(clone, array);

            for (int x = 0; x < 2; ++x)
                for (int y = 0; y < 3; ++y)
                    Assert.AreEqual(clone[x, y], array[x, y]);
        }
    }
}