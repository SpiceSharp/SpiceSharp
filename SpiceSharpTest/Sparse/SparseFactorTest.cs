using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.NewSparse;

namespace SpiceSharpTest.Sparse
{
    [TestClass]
    public class SparseFactorTest
    {
        [TestMethod]
        public void TestFactor1()
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
            Matrix<double> matrix = new Matrix<double>();
            for (int r = 0; r < matrixElements.Length; r++)
                for (int c = 0; c < matrixElements[r].Length; c++)
                    matrix.GetElement(r + 1, c + 1).Value = matrixElements[r][c];

            // Factor
            var solver = new Solver<double>(matrix);
            solver.Factor();

            // compare
            for (int r = 0; r < matrixElements.Length; r++)
                for (int c = 0; c < matrixElements[r].Length; c++)
                    Assert.AreEqual(expected[r][c], matrix.FindElement(r + 1, c + 1).Value, 1e-12);
        }
    }
}
