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
            double[][] matrix =
            [
                [1.0, 1.0, 1.0, 1.0],
                [0.0, 2.0, 0.0, 0.0],
                [0.0, 0.0, 4.0, 0.0],
                [0.0, 0.0, 0.0, 8.0]
            ];

            var solver = new DenseRealSolver();
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    solver[r + 1, c + 1] = matrix[r][c];
            solver[1] = 2;
            solver[2] = 4;
            solver[3] = 5;
            solver[4] = 8;

            var solution = new DenseVector<double>(4);
            Assert.That(solver.OrderAndFactor(), Is.EqualTo(4));
            solver.ForwardSubstitute(solution);
            solver.BackwardSubstitute(solution);

            Assert.That(solution[1], Is.EqualTo(-9.0 / 4.0).Within(1e-12));
            Assert.That(solution[2], Is.EqualTo(2.0).Within(1e-12));
            Assert.That(solution[3], Is.EqualTo(5.0 / 4.0).Within(1e-12));
            Assert.That(solution[4], Is.EqualTo(1.0).Within(1e-12));
        }

        [Test]
        public void When_GearExample_Expect_Reference()
        {
            var solver = new DenseRealSolver();
            for (int i = 1; i <= 4; i++)
                solver[1, i] = 1;
            for (int i = 2; i <= 4; i++)
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
            solver.ForwardSubstitute(sol);
            solver.BackwardSubstitute(sol);
            double[] reference = [5.595238095238093e+07, -1.749999999999999e+08, 2.333333333333332e+08, -1.142857142857142e+08];

            for (int i = 0; i < sol.Length; i++)
            {
                double tol = Math.Max(Math.Abs(sol[i + 1]), Math.Abs(reference[i])) * 1e-12;
                Assert.That(sol[i + 1], Is.EqualTo(reference[i]).Within(tol));
            }
        }
    }
}
