using NUnit.Framework;
using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Solve;
using SpiceSharp.Simulations;
using System;
using System.IO;
using System.Numerics;

namespace SpiceSharpTest.Algebra
{
    [TestFixture]
    public class SparseSolverTests : SolveFramework
    {
        [Test]
        public void When_BigMatrix_Expect_NoException()
        {
            // Test factoring a big matrix
            var solver = new SparseRealSolver();
            ReadMatrix(solver, Path.Combine(TestContext.CurrentContext.TestDirectory, Path.Combine("Algebra", "Matrices", "fidapm05")));

            // Order and factor this larger matrix
            Assert.AreEqual(solver.Size, solver.OrderAndFactor());
        }

        [Test]
        public void When_Spice3f5Reference01_Expect_NoException()
        {
            // Load a matrix from Spice 3f5
            var solver = ReadSpice3f5File(
                Path.Combine(TestContext.CurrentContext.TestDirectory, Path.Combine("Algebra", "Matrices", "spice3f5_matrix01.dat")),
                Path.Combine(TestContext.CurrentContext.TestDirectory, Path.Combine("Algebra", "Matrices", "spice3f5_vector01.dat")));

            // Order and factor
            ModifiedNodalAnalysisHelper<double>.Magnitude = Math.Abs;
            solver.Precondition((matrix, vector) => ModifiedNodalAnalysisHelper<double>.PreorderModifiedNodalAnalysis(matrix, matrix.Size));
            Assert.AreEqual(solver.Size, solver.OrderAndFactor());

            IVector<double> solution = new DenseVector<double>(solver.Size);
            solver.Solve(solution);
        }

        [Test]
        public void When_SingletonPivoting_Expect_NoException()
        {
            // Build the solver with only the singleton pivoting
            var solver = new SparseRealSolver();
            solver.Parameters.Strategies.Clear();
            solver.Parameters.Strategies.Add(new MarkowitzSingleton<double>());

            // Build the matrix that should be solvable using only the singleton pivoting strategy
            double[][] matrix =
            {
                new double[] { 0, 0, 1, 0 },
                new double[] { 1, 1, 1, 1 },
                new double[] { 0, 0, 0, 1 },
                new double[] { 1, 0, 0, 0 }
            };
            double[] rhs = { 0, 1, 0, 0 };
            for (var r = 0; r < matrix.Length; r++)
            {
                for (var c = 0; c < matrix[r].Length; c++)
                {
                    if (!matrix[r][c].Equals(0.0))
                        solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
                }
                if (!rhs[r].Equals(0.0))
                    solver.GetElement(r + 1).Value = rhs[r];
            }

            // This should run without throwing an exception
            Assert.AreEqual(solver.Size, solver.OrderAndFactor());
        }

        [Test]
        public void When_QuickDiagonalPivoting_Expect_NoException()
        {
            // Build the solver with only the quick diagonal pivoting
            var solver = new SparseRealSolver();
            var strategy = solver.Parameters;
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
                        solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
                }
                if (!rhs[r].Equals(0.0))
                    solver.GetElement(r + 1).Value = rhs[r];
            }

            // This should run without throwing an exception
            Assert.AreEqual(solver.Size, solver.OrderAndFactor());
        }

        [Test]
        public void When_DiagonalPivoting_Expect_NoException()
        {
            // Build the solver with only the quick diagonal pivoting
            var solver = new SparseRealSolver();
            var strategy = solver.Parameters;
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
                        solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
                }
                if (!rhs[r].Equals(0.0))
                    solver.GetElement(r + 1).Value = rhs[r];
            }

            // This should run without throwing an exception
            Assert.AreEqual(solver.Size, solver.OrderAndFactor());
        }

        [Test]
        public void When_EntireMatrixPivoting_Expect_NoException()
        {
            // Build the solver with only the quick diagonal pivoting
            var solver = new SparseRealSolver();
            var strategy = solver.Parameters;
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
                        solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
                }
                if (!rhs[r].Equals(0.0))
                    solver.GetElement(r + 1).Value = rhs[r];
            }

            // This should run without throwing an exception
            Assert.AreEqual(solver.Size, solver.OrderAndFactor());
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
            var solver = new SparseComplexSolver();
            for (var r = 0; r < matrix.Length; r++)
            {
                for (var c = 0; c < matrix[r].Length; c++)
                {
                    if (!matrix[r][c].Equals(Complex.Zero))
                        solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrix[r][c];
                }
            }

            // Add some zero elements
            solver.GetElement(new MatrixLocation(7, 7));
            solver.GetElement(5);

            // Build the Rhs vector
            for (var r = 0; r < rhs.Length; r++)
            {
                if (!rhs[r].Equals(Complex.Zero))
                    solver.GetElement(r + 1).Value = rhs[r];
            }

            // Solver
            Assert.AreEqual(solver.Size, solver.OrderAndFactor());
            var solution = new DenseVector<Complex>(solver.Size);
            solver.Solve(solution);

            // Check!
            for (var r = 0; r < reference.Length; r++)
            {
                Assert.AreEqual(reference[r].Real, solution[r + 1].Real, 1e-12);
                Assert.AreEqual(reference[r].Imaginary, solution[r + 1].Imaginary, 1e-12);
            }
        }

        [Test]
        public void When_PartialDecomposition_Expect_Reference()
        {
            var solver = new SparseRealSolver
            {
                PivotSearchReduction = 2, // Limit to only the 2 first elements
                Degeneracy = 2 // Only perform elimination on the first two rows
            };

            solver[1, 2] = 2;
            solver[2, 1] = 1;
            solver[1, 3] = 4;
            solver[4, 2] = 4;
            solver[3, 3] = 2;
            solver[3, 4] = 4;
            solver[4, 4] = 1;

            Assert.AreEqual(2, solver.OrderAndFactor());

            // We are testing two things here:
            // - First, the solver should not have chosen a pivot in the lower-right submatrix
            // - Second, the submatrix should be equal to A_cc - A_c1 * A^-1 * A_1c with A the top-left 
            //   matrix, A_cc the bottom-right submatrix, A_1c and A_c1 the off-diagonal matrices
            Assert.AreEqual(2.0, solver[3, 3], 1e-12);
            Assert.AreEqual(4.0, solver[3, 4], 1e-12);
            Assert.AreEqual(-8.0, solver[4, 3], 1e-12);
            Assert.AreEqual(1.0, solver[4, 4], 1e-12);
        }

        [Test]
        public void When_PartialSolve_Expect_Reference()
        {
            var solver = new SparseRealSolver
            {
                PivotSearchReduction = 2, // Limit to only the 2 first elements
                Degeneracy = 2 // Only perform elimination on the first two rows
            };

            solver[1, 1] = 1;
            solver[1, 3] = 2;
            solver[1, 4] = 3;
            solver[1] = 1;
            solver[2, 2] = 2;
            solver[2, 3] = 4;
            solver[2] = 2;
            solver.Factor();

            // We should now be able to solve for multiple solutions, where the last two elements
            // will determine the result.
            var solution = new DenseVector<double>(4);
            solution[3] = 0;
            solution[4] = 0;
            solver.Solve(solution);
            Assert.AreEqual(1.0, solution[1], 1e-12);
            Assert.AreEqual(1.0, solution[2], 1e-12);
            solution[3] = 1.0;
            solution[4] = 2.0;
            solver.Solve(solution);
            Assert.AreEqual(-7.0, solution[1], 1e-12);
            Assert.AreEqual(-1.0, solution[2], 1e-12);
        }
    }
}
