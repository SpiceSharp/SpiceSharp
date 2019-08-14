using System;
using System.IO;
using System.Numerics;
using NUnit.Framework;
using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Solve;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Sparse
{
    [TestFixture]
    public class SparseSolveTest : SolveFramework
    {
        [Test]
        public void When_BigMatrix_Expect_NoException()
        {
            // Test factoring a big matrix
            var solver = ReadMtxFile(Path.Combine(TestContext.CurrentContext.TestDirectory, Path.Combine("Algebra", "Matrices", "fidapm05.mtx")));

            // Order and factor this larger matrix
            solver.OrderAndFactor();
        }

        [Test]
        public void When_Spice3f5Reference01_Expect_NoException()
        {
            // Load a matrix from Spice 3f5
            var solver = ReadSpice3f5File(
                Path.Combine(TestContext.CurrentContext.TestDirectory, Path.Combine("Algebra", "Matrices", "spice3f5_matrix01.dat")),
                Path.Combine(TestContext.CurrentContext.TestDirectory, Path.Combine("Algebra", "Matrices", "spice3f5_vector01.dat")));

            // Order and factor
            solver.PreorderModifiedNodalAnalysis(Math.Abs);
            solver.OrderAndFactor();

            Vector<double> solution = new DenseVector<double>(solver.Order);
            solver.Solve(solution);
        }

        [Test]
        public void When_SingletonPivoting_Expect_NoException()
        {
            // Build the solver with only the singleton pivoting
            var solver = new RealSolver();
            var strategy = (Markowitz<double>)solver.Strategy;
            strategy.Strategies.Clear();
            strategy.Strategies.Add(new MarkowitzSingleton<double>());

            // Build the matrix that should be solvable using only the singleton pivoting strategy
            double[][] matrix =
            {
                new double[] { 0, 0, 1, 0 },
                new double[] { 1, 1, 1, 1 },
                new double[] { 0, 0, 0, 1 },
                new double[] { 1, 0, 0, 0 }
            };
            double[] rhs = { 0, 1, 0, 0};
            for (var r = 0; r < matrix.Length; r++)
            {
                for (var c = 0; c < matrix[r].Length; c++)
                {
                    if (!matrix[r][c].Equals(0.0))
                        solver.GetMatrixElement(r + 1, c + 1).Value = matrix[r][c];
                }
                if (!rhs[r].Equals(0.0))
                    solver.GetRhsElement(r + 1).Value = rhs[r];
            }

            // This should run without throwing an exception
            solver.OrderAndFactor();
        }

        [Test]
        public void When_QuickDiagonalPivoting_Expect_NoException()
        {
            // Build the solver with only the quick diagonal pivoting
            var solver = new RealSolver();
            var strategy = (Markowitz<double>)solver.Strategy;
            strategy.Strategies.Clear();
            strategy.Strategies.Add(new MarkowitzQuickDiagonal<double>());

            // Build the matrix that should be solvable using only the singleton pivoting strategy
            double[][] matrix =
            {
                new[] {    1,    0.5,   0,      0 },
                new[] { -0.5,      5,   4,      0 },
                new[] {    0,      3,   2,    0.1 },
                new[] {    0,      0,  -0.01,   3 }
            };
            double[] rhs = { 0, 0, 0, 0 };
            for (var r = 0; r < matrix.Length; r++)
            {
                for (var c = 0; c < matrix[r].Length; c++)
                {
                    if (!matrix[r][c].Equals(0.0))
                        solver.GetMatrixElement(r + 1, c + 1).Value = matrix[r][c];
                }
                if (!rhs[r].Equals(0.0))
                    solver.GetRhsElement(r + 1).Value = rhs[r];
            }

            // This should run without throwing an exception
            solver.OrderAndFactor();
        }

        [Test]
        public void When_DiagonalPivoting_Expect_NoException()
        {
            // Build the solver with only the quick diagonal pivoting
            var solver = new RealSolver();
            var strategy = (Markowitz<double>)solver.Strategy;
            strategy.Strategies.Clear();
            strategy.Strategies.Add(new MarkowitzDiagonal<double>());

            // Build the matrix that should be solvable using only the singleton pivoting strategy
            double[][] matrix =
            {
                new[] {    1,    0.5,   0,      0 },
                new double[] {    0,      5,   4,      0 },
                new double[] {    0,      3,   2,      0 },
                new[] {    0,      0,  -0.01,   3 }
            };
            double[] rhs = { 1, 0, 0, 0 };
            for (var r = 0; r < matrix.Length; r++)
            {
                for (var c = 0; c < matrix[r].Length; c++)
                {
                    if (!matrix[r][c].Equals(0.0))
                        solver.GetMatrixElement(r + 1, c + 1).Value = matrix[r][c];
                }
                if (!rhs[r].Equals(0.0))
                    solver.GetRhsElement(r + 1).Value = rhs[r];
            }

            // This should run without throwing an exception
            solver.OrderAndFactor();
        }

        [Test]
        public void When_EntireMatrixPivoting_Expect_NoException()
        {
            // Build the solver with only the quick diagonal pivoting
            var solver = new RealSolver();
            var strategy = (Markowitz<double>)solver.Strategy;
            strategy.Strategies.Clear();
            strategy.Strategies.Add(new MarkowitzEntireMatrix<double>());

            // Build the matrix that should be solvable using only the singleton pivoting strategy
            double[][] matrix =
            {
                new[] {    1,    0.5,      0,  2 },
                new double[] {    2,      5,      4,  3 },
                new double[] {    0,      3,      2,  0 },
                new[] {    4,    1.8,  -0.01,  8 }
            };
            double[] rhs = { 1, 2, 3, 4 };
            for (var r = 0; r < matrix.Length; r++)
            {
                for (var c = 0; c < matrix[r].Length; c++)
                {
                    if (!matrix[r][c].Equals(0.0))
                        solver.GetMatrixElement(r + 1, c + 1).Value = matrix[r][c];
                }
                if (!rhs[r].Equals(0.0))
                    solver.GetRhsElement(r + 1).Value = rhs[r];
            }

            // This should run without throwing an exception
            solver.OrderAndFactor();
        }

        [Test]
        public void When_ExampleComplexMatrix1_Expect_MatlabReference()
        {
            // Build the example matrix
            Complex[][] matrix =
            {
                new Complex[] { 0, 0, 0, 0, 1, 0, 1, 0 },
                new Complex[] { 0, 0, 0, 0, -1, 1, 0, 0 },
                new[] { 0, 0, new Complex(0.0, 0.000628318530717959), 0, 0, 0, -1, 1 },
                new Complex[] { 0, 0, 0, 0.001, 0, 0, 0, -1 },
                new Complex[] { 1, -1, 0, 0, 0, 0, 0, 0 },
                new Complex[] { 0, 1, 0, 0, 0, 0, 0, 0 },
                new Complex[] { 1, 0, -1, 0, 0, 0, 0, 0 },
                new[] { 0, 0, 1, -1, 0, 0, 0, new Complex(0.0, -1.5707963267949) }
            };
            Complex[] rhs = { 0, 0, 0, 0, 0, 24.0 };
            Complex[] reference =
            {
                new Complex(24, 0),
                new Complex(24, 0),
                new Complex(24, 0),
                new Complex(23.999940782519708, -0.037699018824477),
                new Complex(-0.023999940782520, -0.015041945718407),
                new Complex(-0.023999940782520, -0.015041945718407),
                new Complex(0.023999940782520, 0.015041945718407),
                new Complex(0.023999940782520, -0.000037699018824)
            };

            // build the matrix
            var solver = new ComplexSolver();
            for (var r = 0; r < matrix.Length; r++)
            {
                for (var c = 0; c < matrix[r].Length; c++)
                {
                    if (!matrix[r][c].Equals(Complex.Zero))
                        solver.GetMatrixElement(r + 1, c + 1).Value = matrix[r][c];
                }
            }

            // Add some zero elements
            solver.GetMatrixElement(7, 7);
            solver.GetRhsElement(5);

            // Build the Rhs vector
            for (var r = 0; r < rhs.Length; r++)
            {
                if (!rhs[r].Equals(Complex.Zero))
                    solver.GetRhsElement(r + 1).Value = rhs[r];
            }

            // Solver
            solver.OrderAndFactor();
            var solution = new DenseVector<Complex>(solver.Order);
            solver.Solve(solution);

            // Check!
            for (var r = 0; r < reference.Length; r++)
            {
                Assert.AreEqual(reference[r].Real, solution[r + 1].Real, 1e-12);
                Assert.AreEqual(reference[r].Imaginary, solution[r + 1].Imaginary, 1e-12);
            }
        }
    }
}
