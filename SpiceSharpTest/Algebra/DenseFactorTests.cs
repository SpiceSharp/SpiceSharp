using NUnit.Framework;
using SpiceSharp.Algebra;

namespace SpiceSharpTest.Algebra
{
    [TestFixture]
    public class DenseFactorTests
    {
        [Test]
        public void When_Factoring_Expect_Reference()
        {
            double[][] matrixElements =
            {
                new[] { 1.0, 1.0, 1.0 },
                new[] { 2.0, 3.0, 5.0 },
                new[] { 4.0, 6.0, 8.0 }
            };
            double[][] expected =
            {
                new[] { 1.0, 1.0, 1.0 },
                new[] { 2.0, 1.0, 3.0 },
                new[] { 4.0, 2.0, -0.5 }
            };

            var solver = new DenseRealSolver();
            for (var r = 0; r < matrixElements.Length; r++)
                for (var c = 0; c < matrixElements[r].Length; c++)
                    solver[r + 1, c + 1] = matrixElements[r][c];

            // Factor
            solver.Factor();

            // Compare
            for (var r = 0; r < matrixElements.Length; r++)
                for (var c = 0; c < matrixElements[r].Length; c++)
                    Assert.AreEqual(expected[r][c], solver[r + 1, c + 1], 1e-12);
        }

        [Test]
        public void When_OrderAndFactoring_Expect_Reference()
        {
            var solver = new DenseRealSolver();
            solver[1, 1] = 1;
            solver[1, 4] = -1;
            solver[2, 1] = -1;
            solver[2, 3] = 1;
            solver[3, 2] = 1;
            solver[4, 5] = 1;
            solver[5, 4] = 1;

            // Order and factor
            Assert.AreEqual(solver.Size, solver.OrderAndFactor());

            // Compare
            Assert.AreEqual(solver[1, 1], 1.0);
            Assert.AreEqual(solver[1, 4], -1.0);
            Assert.AreEqual(solver[2, 1], -1.0);
            Assert.AreEqual(solver[2, 3], 1.0);
            Assert.AreEqual(solver[2, 4], -1.0);
            Assert.AreEqual(solver[3, 2], 1.0);
            Assert.AreEqual(solver[3, 2], 1.0);
            Assert.AreEqual(solver[4, 5], 1.0);
            Assert.AreEqual(solver[5, 4], 1.0);
        }
    }
}
