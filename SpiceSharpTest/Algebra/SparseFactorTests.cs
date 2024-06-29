// using System;

using NUnit.Framework;
using SpiceSharp.Algebra;
using System;

namespace SpiceSharpTest.Algebra
{
    [TestFixture]
    public class SparseFactorTests
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

            // Create matrix
            var solver = new SparseRealSolver();
            for (int r = 0; r < matrixElements.Length; r++)
                for (int c = 0; c < matrixElements[r].Length; c++)
                    solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value = matrixElements[r][c];

            // Factor
            solver.Factor();

            // Compare
            for (int r = 0; r < matrixElements.Length; r++)
                for (int c = 0; c < matrixElements[r].Length; c++)
                    Assert.That(solver.GetElement(new MatrixLocation(r + 1, c + 1)).Value, Is.EqualTo(expected[r][c]).Within(1e-12));
        }

        [Test]
        public void When_OrderAndFactoring_Expect_Reference()
        {
            var solver = new SparseRealSolver();
            solver.GetElement(new MatrixLocation(1, 1)).Value = 0.0001;
            solver.GetElement(new MatrixLocation(1, 4)).Value = -0.0001;
            solver.GetElement(new MatrixLocation(1, 5)).Value = 0.0;
            solver.GetElement(new MatrixLocation(2, 1)).Value = 0.0;
            solver.GetElement(new MatrixLocation(2, 2)).Value = 1.0;
            solver.GetElement(new MatrixLocation(2, 5)).Value = 0.0;
            solver.GetElement(new MatrixLocation(3, 1)).Value = -0.0001;
            solver.GetElement(new MatrixLocation(3, 3)).Value = 1.0;
            solver.GetElement(new MatrixLocation(3, 4)).Value = 0.0001;
            solver.GetElement(new MatrixLocation(4, 4)).Value = 1.0;
            solver.GetElement(new MatrixLocation(5, 5)).Value = 1.0;

            // Order and factor
            Assert.That(solver.OrderAndFactor(), Is.EqualTo(5));

            // Compare
            Assert.That(solver.GetElement(new MatrixLocation(1, 1)).Value, Is.EqualTo(1.0e4));
            Assert.That(solver.GetElement(new MatrixLocation(1, 4)).Value, Is.EqualTo(-0.0001));
            Assert.That(solver.GetElement(new MatrixLocation(1, 5)).Value, Is.EqualTo(0.0));
            Assert.That(solver.GetElement(new MatrixLocation(2, 1)).Value, Is.EqualTo(0.0));
            Assert.That(solver.GetElement(new MatrixLocation(2, 2)).Value, Is.EqualTo(1.0));
            Assert.That(solver.GetElement(new MatrixLocation(2, 5)).Value, Is.EqualTo(0.0));
            Assert.That(solver.GetElement(new MatrixLocation(3, 1)).Value, Is.EqualTo(-0.0001));
            Assert.That(solver.GetElement(new MatrixLocation(3, 3)).Value, Is.EqualTo(1.0));
            Assert.That(solver.GetElement(new MatrixLocation(3, 4)).Value, Is.EqualTo(0.0001));
            Assert.That(solver.GetElement(new MatrixLocation(4, 4)).Value, Is.EqualTo(1.0));
            Assert.That(solver.GetElement(new MatrixLocation(5, 5)).Value, Is.EqualTo(1.0));
        }

        [Test]
        public void When_OrderAndFactoring2_Expect_Reference()
        {
            var solver = new SparseRealSolver();
            solver.GetElement(new MatrixLocation(1, 1)).Value = 1.0;
            solver.GetElement(new MatrixLocation(2, 1)).Value = 0.0;
            solver.GetElement(new MatrixLocation(2, 2)).Value = 1.0;
            solver.GetElement(new MatrixLocation(2, 5)).Value = 0.0;
            solver.GetElement(new MatrixLocation(3, 3)).Value = 1.0;
            solver.GetElement(new MatrixLocation(3, 4)).Value = 1e-4;
            solver.GetElement(new MatrixLocation(3, 5)).Value = -1e-4;
            solver.GetElement(new MatrixLocation(4, 4)).Value = 1.0;
            solver.GetElement(new MatrixLocation(5, 1)).Value = 5.38e-23;
            solver.GetElement(new MatrixLocation(5, 4)).Value = -1e-4;
            solver.GetElement(new MatrixLocation(5, 5)).Value = 1e-4;

            Assert.That(solver.OrderAndFactor(), Is.EqualTo(5));

            AssertInternal(solver, 1, 1, 1.0);
            AssertInternal(solver, 2, 1, 0.0);
            AssertInternal(solver, 2, 2, 1.0);
            AssertInternal(solver, 2, 5, 0.0);
            AssertInternal(solver, 3, 3, 1.0);
            AssertInternal(solver, 3, 4, 1e-4);
            AssertInternal(solver, 3, 5, -1e-4);
            AssertInternal(solver, 4, 4, 1.0);
            AssertInternal(solver, 5, 1, 5.38e-23);
            AssertInternal(solver, 5, 4, -1e-4);
            AssertInternal(solver, 5, 5, 10000);
        }

        [Test]
        public void When_Preorder_Expect_Reference()
        {
            var solver = new SparseRealSolver();
            solver.GetElement(new MatrixLocation(1, 1)).Value = 1e-4;
            solver.GetElement(new MatrixLocation(1, 2)).Value = 0.0;
            solver.GetElement(new MatrixLocation(1, 3)).Value = -1e-4;
            solver.GetElement(new MatrixLocation(2, 1)).Value = 0.0;
            solver.GetElement(new MatrixLocation(2, 2)).Value = 0.0;
            solver.GetElement(new MatrixLocation(2, 5)).Value = 1.0;
            solver.GetElement(new MatrixLocation(3, 1)).Value = -1e-4;
            solver.GetElement(new MatrixLocation(3, 3)).Value = 1e-4;
            solver.GetElement(new MatrixLocation(3, 4)).Value = 1.0;
            solver.GetElement(new MatrixLocation(4, 3)).Value = 1.0;
            solver.GetElement(new MatrixLocation(5, 2)).Value = 1.0;

            SpiceSharp.Simulations.ModifiedNodalAnalysisHelper<double>.Magnitude = Math.Abs;
            solver.Precondition((matrix, vector) => SpiceSharp.Simulations.ModifiedNodalAnalysisHelper<double>.PreorderModifiedNodalAnalysis(matrix, matrix.Size));

            AssertInternal(solver, 1, 1, 1e-4);
            AssertInternal(solver, 1, 4, -1e-4);
            AssertInternal(solver, 1, 5, 0.0);
            AssertInternal(solver, 2, 1, 0.0);
            AssertInternal(solver, 2, 2, 1.0);
            AssertInternal(solver, 2, 5, 0.0);
            AssertInternal(solver, 3, 1, -1e-4);
            AssertInternal(solver, 3, 3, 1.0);
            AssertInternal(solver, 3, 4, 1e-4);
            AssertInternal(solver, 4, 4, 1.0);
            AssertInternal(solver, 5, 5, 1.0);
        }

        [Test]
        public void When_PartialDecompositionSingular_Expect_Reference()
        {
            var solver = new SparseRealSolver();
            solver[1, 1] = 1;
            solver[2, 2] = 1;
            solver[2, 3] = 1;
            solver[3, 1] = 1;
            solver[3, 2] = 1;
            solver[3, 3] = 1;

            solver.Degeneracy = 1;
            Assert.That(solver.Factor(), Is.EqualTo(true));

            AssertInternal(solver, 1, 1, 1);
            AssertInternal(solver, 2, 2, 1);
            AssertInternal(solver, 2, 3, 1);
            AssertInternal(solver, 3, 1, 1);
            AssertInternal(solver, 3, 2, 1);
            AssertInternal(solver, 3, 3, 0);
        }

        /// <summary>
        /// Assert internal element
        /// </summary>
        /// <param name="solver"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="expected"></param>
        void AssertInternal(ISparsePivotingSolver<double> solver, int row, int col, double expected)
        {
            var indices = new MatrixLocation(row, col);
            indices = solver.InternalToExternal(indices);
            var elt = solver.FindElement(indices);
            Assert.That(elt, Is.Not.Null);
            Assert.That(elt.Value, Is.EqualTo(expected));
        }
    }
}
