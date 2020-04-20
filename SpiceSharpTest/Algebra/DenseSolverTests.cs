using NUnit.Framework;
using SpiceSharp.Algebra;
using System;

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

            var solver = new DenseRealSolver();
            for (var r = 0; r < 4; r++)
                for (var c = 0; c < 4; c++)
                    solver[r + 1, c + 1] = matrix[r][c];
            solver[1] = 2;
            solver[2] = 4;
            solver[3] = 5;
            solver[4] = 8;

            var solution = new DenseVector<double>(4);
            Assert.AreEqual(4, solver.OrderAndFactor());
            solver.Solve(solution);

            Assert.AreEqual(solution[1], -9.0 / 4.0, 1e-12);
            Assert.AreEqual(solution[2], 2.0, 1e-12);
            Assert.AreEqual(solution[3], 5.0 / 4.0, 1e-12);
            Assert.AreEqual(solution[4], 1.0, 1e-12);
        }

        [Test]
        public void When_GearExample_Expect_Reference()
        {
            var solver = new DenseRealSolver();
            for (var i = 1; i <= 4; i++)
                solver[1, i] = 1;
            for (var i = 2; i <= 4; i++)
                solver[i, 2] = 1;
            solver[2, 3] = 1.5;
            solver[3, 3] = 2.25;
            solver[4, 3] = 3.375;
            solver[2, 4] = 1.75;
            solver[3, 4] = 3.0625;
            solver[4, 4] = 5.359375;
            solver[2] = -25000000;

            solver.Factor();
            var sol = new DenseVector<double>(4);
            solver.Solve(sol);
            var reference = new double[] { 5.595238095238093e+07, -1.749999999999999e+08, 2.333333333333332e+08, -1.142857142857142e+08 };

            for (var i = 0; i < sol.Length; i++)
            {
                var tol = Math.Max(Math.Abs(sol[i + 1]), Math.Abs(reference[i])) * 1e-12;
                Assert.AreEqual(reference[i], sol[i + 1], tol);
            }
        }
    }
}
