using NUnit.Framework;
using SpiceSharp.Algebra;

namespace SpiceSharpTest.Sparse
{
    [TestFixture]
    public class SparseFactorTest
    {
        [Test]
        public void When_Factoring_Expect_Reference()
        {
            double[][] matrixElements =
            {
                new double[] { 1.0, 1.0, 1.0 },
                new double[] { 2.0, 3.0, 5.0 },
                new double[] { 4.0, 6.0, 8.0 }
            };
            double[][] expected =
            {
                new double[] { 1.0, 1.0, 1.0 },
                new double[] { 2.0, 1.0, 3.0 },
                new double[] { 4.0, 2.0, -0.5 }
            };

            // Create matrix
            var solver = new RealSolver();
            for (int r = 0; r < matrixElements.Length; r++)
                for (int c = 0; c < matrixElements[r].Length; c++)
                    solver.GetMatrixElement(r + 1, c + 1).Value = matrixElements[r][c];

            // Factor
            solver.Factor();

            // compare
            for (int r = 0; r < matrixElements.Length; r++)
                for (int c = 0; c < matrixElements[r].Length; c++)
                    Assert.AreEqual(expected[r][c], solver.GetMatrixElement(r + 1, c + 1).Value, 1e-12);
        }
    }
}
