// using System;

using System;
using NUnit.Framework;
using SpiceSharp.Algebra;

namespace SpiceSharpTest.Algebra
{
    [TestFixture]
    public class SparseFactorTests
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

            // Create matrix
            var solver = new SparseRealSolver<SparseMatrix<double>, SparseVector<double>>(
                new SparseMatrix<double>(),
                new SparseVector<double>()
                );
            for (var r = 0; r < matrixElements.Length; r++)
                for (var c = 0; c < matrixElements[r].Length; c++)
                    solver.GetMatrixElement(r + 1, c + 1).Value = matrixElements[r][c];

            // Factor
            solver.Factor();

            // Compare
            for (var r = 0; r < matrixElements.Length; r++)
                for (var c = 0; c < matrixElements[r].Length; c++)
                    Assert.AreEqual(expected[r][c], solver.GetMatrixElement(r + 1, c + 1).Value, 1e-12);
        }

        [Test]
        public void When_OrderAndFactoring_Expect_Reference()
        {
            var solver = new SparseRealSolver<SparseMatrix<double>, SparseVector<double>>(
                new SparseMatrix<double>(),
                new SparseVector<double>()
                );
            solver.GetMatrixElement(1, 1).Value = 0.0001;
            solver.GetMatrixElement(1, 4).Value = -0.0001;
            solver.GetMatrixElement(1, 5).Value = 0.0;
            solver.GetMatrixElement(2, 1).Value = 0.0;
            solver.GetMatrixElement(2, 2).Value = 1.0;
            solver.GetMatrixElement(2, 5).Value = 0.0;
            solver.GetMatrixElement(3, 1).Value = -0.0001;
            solver.GetMatrixElement(3, 3).Value = 1.0;
            solver.GetMatrixElement(3, 4).Value = 0.0001;
            solver.GetMatrixElement(4, 4).Value = 1.0;
            solver.GetMatrixElement(5, 5).Value = 1.0;
            
            // Order and factor
            solver.OrderAndFactor();

            // Compare
            Assert.AreEqual(solver.GetMatrixElement(1, 1).Value, 1.0e4);
            Assert.AreEqual(solver.GetMatrixElement(1, 4).Value, -0.0001);
            Assert.AreEqual(solver.GetMatrixElement(1, 5).Value, 0.0);
            Assert.AreEqual(solver.GetMatrixElement(2, 1).Value, 0.0);
            Assert.AreEqual(solver.GetMatrixElement(2, 2).Value, 1.0);
            Assert.AreEqual(solver.GetMatrixElement(2, 5).Value, 0.0);
            Assert.AreEqual(solver.GetMatrixElement(3, 1).Value, -0.0001);
            Assert.AreEqual(solver.GetMatrixElement(3, 3).Value, 1.0);
            Assert.AreEqual(solver.GetMatrixElement(3, 4).Value, 0.0001);
            Assert.AreEqual(solver.GetMatrixElement(4, 4).Value, 1.0);
            Assert.AreEqual(solver.GetMatrixElement(5, 5).Value, 1.0);
        }

        [Test]
        public void When_OrderAndFactoring2_Expect_Reference()
        {
            var solver = new SparseRealSolver<SparseMatrix<double>, SparseVector<double>>(
                new SparseMatrix<double>(5),
                new SparseVector<double>(5)
                );

            solver.GetMatrixElement(1, 1).Value = 1.0;
            solver.GetMatrixElement(2, 1).Value = 0.0;
            solver.GetMatrixElement(2, 2).Value = 1.0;
            solver.GetMatrixElement(2, 5).Value = 0.0;
            solver.GetMatrixElement(3, 3).Value = 1.0;
            solver.GetMatrixElement(3, 4).Value = 1e-4;
            solver.GetMatrixElement(3, 5).Value = -1e-4;
            solver.GetMatrixElement(4, 4).Value = 1.0;
            solver.GetMatrixElement(5, 1).Value = 5.38e-23;
            solver.GetMatrixElement(5, 4).Value = -1e-4;
            solver.GetMatrixElement(5, 5).Value = 1e-4;

            solver.OrderAndFactor();

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
            var solver = new SparseRealSolver<SparseMatrix<double>, SparseVector<double>>(
                new SparseMatrix<double>(5),
                new SparseVector<double>(5)
                );
            solver.GetMatrixElement(1, 1).Value = 1e-4;
            solver.GetMatrixElement(1, 2).Value = 0.0;
            solver.GetMatrixElement(1, 3).Value = -1e-4;
            solver.GetMatrixElement(2, 1).Value = 0.0;
            solver.GetMatrixElement(2, 2).Value = 0.0;
            solver.GetMatrixElement(2, 5).Value = 1.0;
            solver.GetMatrixElement(3, 1).Value = -1e-4;
            solver.GetMatrixElement(3, 3).Value = 1e-4;
            solver.GetMatrixElement(3, 4).Value = 1.0;
            solver.GetMatrixElement(4, 3).Value = 1.0;
            solver.GetMatrixElement(5, 2).Value = 1.0;

            SpiceSharp.Simulations.ModifiedNodalAnalysisHelper<double>.Magnitude = Math.Abs;
            solver.Precondition((matrix, vector) => SpiceSharp.Simulations.ModifiedNodalAnalysisHelper<double>.PreorderModifiedNodalAnalysis(matrix));

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

        /// <summary>
        /// Assert internal element
        /// </summary>
        /// <param name="solver"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="expected"></param>
        void AssertInternal(ISolver<double> solver, int row, int col, double expected)
        {
            var indices = Tuple.Create(row, col);
            indices = solver.InternalToExternal(indices);
            var elt = solver.FindMatrixElement(indices.Item1, indices.Item2);
            Assert.AreNotEqual(null, elt);
            Assert.AreEqual(expected, elt.Value);
        }
    }
}
