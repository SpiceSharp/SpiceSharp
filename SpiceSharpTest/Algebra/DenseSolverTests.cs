using NUnit.Framework;
using SpiceSharp.Algebra;

namespace SpiceSharpTest.Algebra
{
    [TestFixture]
    public class DenseSolverTests
    {
        [Test]
        public void When_SimpleMatrix_Expect_Reference()
        {
            double[][] matrix = new[]
            {
                new[] { 1.0, 1.0, 1.0, 1.0 },
                new[] { 0.0, 2.0, 0.0, 0.0 },
                new[] { 0.0, 0.0, 4.0, 0.0 },
                new[] { 0.0, 0.0, 0.0, 8.0 }
            };

            var solver = new DenseRealSolver<DenseMatrix<double>, DenseVector<double>>(
                new DenseMatrix<double>(),
                new DenseVector<double>()
                );
            for (var r = 0; r < 4; r++)
                for (var c = 0; c < 4; c++)
                    solver[r + 1, c + 1] = matrix[r][c];
            solver[1] = 2;
            solver[2] = 4;
            solver[3] = 5;
            solver[4] = 8;

            var solution = new DenseVector<double>(4);
            solver.OrderAndFactor();
            solver.Solve(solution);

            Assert.AreEqual(solution[1], -9.0 / 4.0, 1e-12);
            Assert.AreEqual(solution[2], 2.0, 1e-12);
            Assert.AreEqual(solution[3], 5.0 / 4.0, 1e-12);
            Assert.AreEqual(solution[4], 1.0, 1e-12);
        }
    }
}
