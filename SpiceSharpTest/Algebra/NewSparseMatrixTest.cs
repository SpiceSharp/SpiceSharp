using NUnit.Framework;
using SpiceSharp.Algebra;

namespace SpiceSharpTest.Sparse
{
    [TestFixture]
    public class SparseMatrixTest
    {
        [Test]
        public void When_BuildingMatrix_Expect_CorrectLinks()
        {
            int size = 20;
            int rStep = 7;
            int cStep = 11;

            var matrix = new SparseMatrix<double>();
            for (int c = 0; c < rStep * size; c += rStep)
            {
                for (int r = 0; r < cStep * size; r += cStep)
                {
                    int row = r % size;
                    int col = c % size;
                    int expected = row * size + col + 1;
                    matrix.GetElement(row + 1, col + 1).Value = expected;
                }
            }

            // Check links from left to right
            MatrixElement<double> element;
            for (int r = 0; r < size; r++)
            {
                element = matrix.GetFirstInRow(r + 1);
                for (int c = 0; c < size; c++)
                {
                    int expected = r * size + c + 1;
                    Assert.AreEqual(expected, element.Value, 1e-12);
                    element = element.Right;
                }
            }

            // Check links from right to left
            for (int r = 0; r < size; r++)
            {
                element = matrix.GetLastInRow(r + 1);
                for (int c = size - 1; c >= 0; c--)
                {
                    int expected = r * size + c + 1;
                    Assert.AreEqual(expected, element.Value, 1e-12);
                    element = element.Left;
                }
            }

            // Check links from top to bottom
            for (int c = 0; c < size; c++)
            {
                element = matrix.GetFirstInColumn(c + 1);
                for (int r = 0; r < size; r++)
                {
                    int expected = r * size + c + 1;
                    Assert.AreEqual(expected, element.Value, 1e-12);
                    element = element.Below;
                }
            }

            // Check links from bottom to top
            for (int c = 0; c < size; c++)
            {
                element = matrix.GetLastInColumn(c + 1);
                for (int r = size - 1; r >= 0; r--)
                {
                    int expected = r * size + c + 1;
                    Assert.AreEqual(expected, element.Value, 1e-12);
                    element = element.Above;
                }
            }
        }

        [Test]
        public void When_SwappingRows_Expect_Reference()
        {
            // Build the matrix
            SparseMatrix<double> matrix = new SparseMatrix<double>();

            // We want to test all possible combinations for the row! We need 5 elements to be able to test it
            for (int i = 0; i < 32; i++)
            {
                // Our counter "i" will represent in binary which elements need to be filled.
                int fill = i;
                for (int k = 0; k < 5; k++)
                {
                    // Get whether or not the element needs to be filled
                    if ((fill & 0x01) != 0)
                    {
                        int expected = k * 32 + i + 1;
                        matrix.GetElement(k + 1, i + 1).Value = expected;
                    }
                    fill = (fill >> 1) & 0b011111;
                }
            }

            // Swap the two rows of interest
            matrix.SwapRows(2, 4);

            // Find the elements back
            for (int i = 0; i < 32; i++)
            {
                int fill = i;
                for (int k = 0; k < 5; k++)
                {
                    // Get the current row to test (remember we swapped them!)
                    int crow = k + 1;
                    if (crow == 2)
                        crow = 4;
                    else if (crow == 4)
                        crow = 2;

                    if ((fill & 0x01) != 0)
                    {
                        int expected = k * 32 + i + 1;
                        Assert.AreEqual(expected, matrix[crow, i + 1], 1e-12);
                    }
                    else
                        Assert.AreEqual(null, matrix.FindElement(crow, i + 1));
                    fill = (fill >> 1) & 0b011111;
                }
            }
        }

        [Test]
        public void When_SwappingColumns_Expect_Reference()
        {
            // Build the matrix
            SparseMatrix<double> matrix = new SparseMatrix<double>();

            // We want to test all possible combinations for the row! We need 5 elements to be able to test it
            for (int i = 0; i < 32; i++)
            {
                // Our counter "i" will represent in binary which elements need to be filled.
                int fill = i;
                for (int k = 0; k < 5; k++)
                {
                    // Get whether or not the element needs to be filled
                    if ((fill & 0x01) != 0)
                    {
                        int expected = k * 32 + i + 1;
                        matrix.GetElement(i + 1, k + 1).Value = expected;
                    }
                    fill = (fill >> 1) & 0b011111;
                }
            }

            // Swap the two rows of interest
            matrix.SwapColumns(2, 4);

            // Find the elements back
            for (int i = 0; i < 32; i++)
            {
                int fill = i;
                for (int k = 0; k < 5; k++)
                {
                    // Get the current row to test (remember we swapped them!)
                    int ccolumn = k + 1;
                    if (ccolumn == 2)
                        ccolumn = 4;
                    else if (ccolumn == 4)
                        ccolumn = 2;

                    if ((fill & 0x01) != 0)
                    {
                        int expected = k * 32 + i + 1;
                        Assert.AreEqual(expected, matrix[i + 1, ccolumn], 1e-12);
                    }
                    else
                        Assert.AreEqual(null, matrix.FindElement(i + 1, ccolumn));
                    fill = (fill >> 1) & 0b011111;
                }
            }
        }
    }
}
