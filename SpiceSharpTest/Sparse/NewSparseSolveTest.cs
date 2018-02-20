using SpiceSharp.NewSparse;
using SpiceSharp.NewSparse.Solve;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SpiceSharpTest.Sparse
{
    [TestClass]
    public class NewSparseSolveTest : SolveFramework
    {
        [TestMethod]
        public void TestFidapm05_mtx()
        {
            /*
             * Test factoring a big matrix
             */

            var solver = ReadMtxFile("Sparse/Matrices/fidapm05.mtx");

            // Order and factor this larger matrix
            solver.OrderAndFactor();
        }

        [TestMethod]
        public void TestSingletonPivot()
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
            for (int r = 0; r < matrix.Length; r++)
            {
                for (int c = 0; c < matrix[r].Length; c++)
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

        [TestMethod]
        public void TestQuickDiagonalPivot()
        {
            // Build the solver with only the quick diagonal pivoting
            var solver = new RealSolver();
            var strategy = (Markowitz<double>)solver.Strategy;
            strategy.Strategies.Clear();
            strategy.Strategies.Add(new MarkowitzQuickDiagonal<double>());

            // Build the matrix that should be solvable using only the singleton pivoting strategy
            double[][] matrix =
            {
                new double[] {    1,    0.5,   0,      0 },
                new double[] { -0.5,      5,   4,      0 },
                new double[] {    0,      3,   2,    0.1 },
                new double[] {    0,      0,  -0.01,   3 }
            };
            double[] rhs = { 0, 0, 0, 0 };
            for (int r = 0; r < matrix.Length; r++)
            {
                for (int c = 0; c < matrix[r].Length; c++)
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

        [TestMethod]
        public void TestDiagonalPivot()
        {
            // Build the solver with only the quick diagonal pivoting
            var solver = new RealSolver();
            var strategy = (Markowitz<double>)solver.Strategy;
            strategy.Strategies.Clear();
            strategy.Strategies.Add(new MarkowitzDiagonal<double>());

            // Build the matrix that should be solvable using only the singleton pivoting strategy
            double[][] matrix =
            {
                new double[] {    1,    0.5,   0,      0 },
                new double[] {    0,      5,   4,      0 },
                new double[] {    0,      3,   2,      0 },
                new double[] {    0,      0,  -0.01,   3 }
            };
            double[] rhs = { 1, 0, 0, 0 };
            for (int r = 0; r < matrix.Length; r++)
            {
                for (int c = 0; c < matrix[r].Length; c++)
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

        [TestMethod]
        public void TestEntireMatrixPivot()
        {
            // Build the solver with only the quick diagonal pivoting
            var solver = new RealSolver();
            var strategy = (Markowitz<double>)solver.Strategy;
            strategy.Strategies.Clear();
            strategy.Strategies.Add(new MarkowitzEntireMatrix<double>());

            // Build the matrix that should be solvable using only the singleton pivoting strategy
            double[][] matrix =
            {
                new double[] {    1,    0.5,      0,  2 },
                new double[] {    2,      5,      4,  3 },
                new double[] {    0,      3,      2,  0 },
                new double[] {    4,    1.8,  -0.01,  8 }
            };
            double[] rhs = { 1, 2, 3, 4 };
            for (int r = 0; r < matrix.Length; r++)
            {
                for (int c = 0; c < matrix[r].Length; c++)
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
    }
}
