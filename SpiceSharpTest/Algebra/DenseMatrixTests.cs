using NUnit.Framework;
using SpiceSharp.Algebra;

namespace SpiceSharpTest.Algebra
{
    [TestFixture]
    public class DenseMatrixTests
    {
        [Test]
        public void When_DenseMatrixExpand_Expect_Reference()
        {
            var n = new DenseMatrix<double>();
            n[10, 10] = 3;

            Assert.AreEqual(10, n.Size);
            Assert.AreEqual(3.0, n[10, 10], 1e-12);

            n.Clear();

            Assert.AreEqual(0, n.Size);
            Assert.AreEqual(0.0, n[10, 10], 1e-12);
        }

        [Test]
        public void When_SwappingRows_Expect_Reference()
        {
            var n = new DenseMatrix<double>();
            for (var r = 1; r < 10; r++)
                for (var c = 1; c < 10; c++)
                    n[r, c] = (r - 1) * 10 + c;

            n.SwapRows(2, 5);

            for (var r = 1; r < 10; r++)
            {
                var row = r == 2 ? 5 : r == 5 ? 2 : r;
                for (var c = 1; c < 10; c++)
                    Assert.AreEqual((row - 1) * 10 + c, n[r, c], 1e-12);
            }

            n.Clear();
        }

        [Test]
        public void When_SwappingColumns_Expect_Reference()
        {
            var n = new DenseMatrix<double>();
            for (var r = 1; r < 10; r++)
                for (var c = 1; c < 10; c++)
                    n[r, c] = (r - 1) * 10 + c;

            n.SwapColumns(2, 5);

            for (var r = 1; r < 10; r++)
            {
                for (var c = 1; c < 10; c++)
                {
                    var col = c == 2 ? 5 : c == 5 ? 2 : c;
                    Assert.AreEqual((r - 1) * 10 + col, n[r, c], 1e-12);
                }
            }

            n.Clear();
        }
    }
}
