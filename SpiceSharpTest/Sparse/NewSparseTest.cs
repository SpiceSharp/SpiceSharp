using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.NewSparse;

namespace SpiceSharpTest.Sparse
{
    [TestClass]
    public class NewSparseTest
    {
        [TestMethod]
        public void TestSparseLinks()
        {
            int size = 20;
            int rStep = 7;
            int cStep = 11;

            var matrix = new Matrix<double>();
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
            MatrixIterator<double> iterator;
            for (int r = 0; r < size; r++)
            {
                iterator = matrix.GetIterator(r + 1, 1);
                for (int c = 0; c < size; c++)
                {
                    int expected = r * size + c + 1;
                    Assert.AreEqual(expected, iterator.Element.Value, 1e-12);
                    iterator.MoveRight();
                }
            }

            // Check links from right to left
            for (int r = 0; r < size; r++)
            {
                iterator = matrix.GetIterator(r + 1, size);
                for (int c = size - 1; c >= 0; c--)
                {
                    int expected = r * size + c + 1;
                    Assert.AreEqual(expected, iterator.Element.Value, 1e-12);
                    iterator.MoveLeft();
                }
            }

            // Check links from top to bottom
            for (int c = 0; c < size; c++)
            {
                iterator = matrix.GetIterator(1, c + 1);
                for (int r = 0; r < size; r++)
                {
                    int expected = r * size + c + 1;
                    Assert.AreEqual(expected, iterator.Element.Value, 1e-12);
                    iterator.MoveDown();
                }
            }

            // Check links from bottom to top
            for (int c = 0; c < size; c++)
            {
                iterator = matrix.GetIterator(size, c + 1);
                for (int r = size - 1; r >= 0; r--)
                {
                    int expected = r * size + c + 1;
                    Assert.AreEqual(expected, iterator.Element.Value, 1e-12);
                    iterator.MoveUp();
                }
            }
        }

        [TestMethod]
        public void TestSwapRows()
        {
            // Build the matrix
            Matrix<double> matrix = new Matrix<double>();

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
                        Assert.AreEqual(expected, matrix.FindElement(crow, i + 1)?.Value);
                    }
                    else
                        Assert.AreEqual(null, matrix.FindElement(crow, i + 1));
                    fill = (fill >> 1) & 0b011111;
                }
            }
        }

        [TestMethod]
        public void TestSwapColumns()
        {
            // Build the matrix
            Matrix<double> matrix = new Matrix<double>();

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
                        Assert.AreEqual(expected, matrix.FindElement(i + 1, ccolumn)?.Value);
                    }
                    else
                        Assert.AreEqual(null, matrix.FindElement(i + 1, ccolumn));
                    fill = (fill >> 1) & 0b011111;
                }
            }
        }
    }
}
