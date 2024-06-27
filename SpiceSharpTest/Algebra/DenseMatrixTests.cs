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

            Assert.That(n.Size, Is.EqualTo(10));
            Assert.That(n[10, 10], Is.EqualTo(3.0).Within(1e-12));

            n.Clear();

            Assert.That(n.Size, Is.EqualTo(0));
            Assert.That(n[10, 10], Is.EqualTo(0.0).Within(1e-12));
        }

        [Test]
        public void When_SwappingRows_Expect_Reference()
        {
            var n = new DenseMatrix<double>();
            for (int r = 1; r < 10; r++)
                for (int c = 1; c < 10; c++)
                    n[r, c] = (r - 1) * 10 + c;

            n.SwapRows(2, 5);

            for (int r = 1; r < 10; r++)
            {
                int row = r == 2 ? 5 : r == 5 ? 2 : r;
                for (int c = 1; c < 10; c++)
                    Assert.That(n[r, c], Is.EqualTo((row - 1) * 10 + c).Within(1e-12));
            }

            n.Clear();
        }

        [Test]
        public void When_SwappingColumns_Expect_Reference()
        {
            var n = new DenseMatrix<double>();
            for (int r = 1; r < 10; r++)
                for (int c = 1; c < 10; c++)
                    n[r, c] = (r - 1) * 10 + c;

            n.SwapColumns(2, 5);

            for (int r = 1; r < 10; r++)
            {
                for (int c = 1; c < 10; c++)
                {
                    int col = c == 2 ? 5 : c == 5 ? 2 : c;
                    Assert.That(n[r, c], Is.EqualTo((r - 1) * 10 + col).Within(1e-12));
                }
            }

            n.Clear();
        }
    }
}
