using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine.ClientProtocol;

namespace Emerita.UnitTests
{
    [TestClass]
    public class PerftTests
    {
        public TestContext? TestContext { get; set; } 

        [TestMethod]
        [DataRow(0, 1ul)]
        [DataRow(1, 20ul)]
        [DataRow(2, 400ul)]
        [DataRow(3, 8902ul)]
        [DataRow(4, 197281ul)]
        [DataRow(5, 4865609ul)]
        [DataRow(6, 119060324ul)]
        public void PerftTest(int depth, ulong expectedNodes)
        {
            Perft perft = new Perft();
            Stopwatch watch = new();
            watch.Start();
            ulong actual = perft.Execute(depth);
            watch.Stop();

            TestContext?.WriteLine($"Elapsed = {watch.Elapsed}");
            Assert.AreEqual(expectedNodes, actual);
        }
    }
}