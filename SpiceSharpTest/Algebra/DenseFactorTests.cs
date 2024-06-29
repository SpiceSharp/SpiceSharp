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
            [
                [1.0, 1.0, 1.0],
                [2.0, 3.0, 5.0],
                [4.0, 6.0, 8.0]
            ];
            double[][] expected =
            [
                [1.0, 1.0, 1.0],
                [2.0, 1.0, 3.0],
                [4.0, 2.0, -0.5]
            ];

            var solver = new DenseRealSolver();
            for (int r = 0; r < matrixElements.Length; r++)
                for (int c = 0; c < matrixElements[r].Length; c++)
                    solver[r + 1, c + 1] = matrixElements[r][c];

            // Factor
            solver.Factor();

            // Compare
            for (int r = 0; r < matrixElements.Length; r++)
                for (int c = 0; c < matrixElements[r].Length; c++)
                    Assert.That(solver[r + 1, c + 1], Is.EqualTo(expected[r][c]).Within(1e-12));
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
            Assert.That(solver.OrderAndFactor(), Is.EqualTo(solver.Size));

            // Compare
            Assert.That(solver[1, 1], Is.EqualTo(1.0));
            Assert.That(solver[1, 4], Is.EqualTo(-1.0));
            Assert.That(solver[2, 1], Is.EqualTo(-1.0));
            Assert.That(solver[2, 3], Is.EqualTo(1.0));
            Assert.That(solver[2, 4], Is.EqualTo(-1.0));
            Assert.That(solver[3, 2], Is.EqualTo(1.0));
            Assert.That(solver[3, 2], Is.EqualTo(1.0));
            Assert.That(solver[4, 5], Is.EqualTo(1.0));
            Assert.That(solver[5, 4], Is.EqualTo(1.0));
        }
    }
}
