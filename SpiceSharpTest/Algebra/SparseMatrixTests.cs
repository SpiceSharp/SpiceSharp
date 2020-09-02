using NUnit.Framework;
using SpiceSharp.Algebra;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharpTest.Algebra
{
    [TestFixture]
    public class SparseMatrixTests
    {
        [Test]
        public void When_BuildingMatrix_Expect_CorrectLinks()
        {
            var size = 20;
            var rStep = 7;
            var cStep = 11;

            var matrix = new SparseMatrix<double>();
            for (var c = 0; c < rStep * size; c += rStep)
            {
                for (var r = 0; r < cStep * size; r += cStep)
                {
                    var row = r % size;
                    var col = c % size;
                    var expected = row * size + col + 1;
                    matrix.GetElement(new MatrixLocation(row + 1, col + 1)).Value = expected;
                }
            }

            // Check links from left to right
            ISparseMatrixElement<double> element;
            for (var r = 0; r < size; r++)
            {
                element = matrix.GetFirstInRow(r + 1);
                for (var c = 0; c < size; c++)
                {
                    var expected = r * size + c + 1;
                    Assert.AreEqual(expected, element.Value, 1e-12);
                    element = element.Right;
                }
            }

            // Check links from right to left
            for (var r = 0; r < size; r++)
            {
                element = matrix.GetLastInRow(r + 1);
                for (var c = size - 1; c >= 0; c--)
                {
                    var expected = r * size + c + 1;
                    Assert.AreEqual(expected, element.Value, 1e-12);
                    element = element.Left;
                }
            }

            // Check links from top to bottom
            for (var c = 0; c < size; c++)
            {
                element = matrix.GetFirstInColumn(c + 1);
                for (var r = 0; r < size; r++)
                {
                    var expected = r * size + c + 1;
                    Assert.AreEqual(expected, element.Value, 1e-12);
                    element = element.Below;
                }
            }

            // Check links from bottom to top
            for (var c = 0; c < size; c++)
            {
                element = matrix.GetLastInColumn(c + 1);
                for (var r = size - 1; r >= 0; r--)
                {
                    var expected = r * size + c + 1;
                    Assert.AreEqual(expected, element.Value, 1e-12);
                    element = element.Above;
                }
            }

            matrix.Clear();
        }

        [Test]
        public void When_SwappingRows_Expect_Reference()
        {
            // Build the matrix
            var matrix = new SparseMatrix<double>();

            // We want to test all possible combinations for the row! We need 5 elements to be able to test it
            for (var i = 0; i < 32; i++)
            {
                // Our counter "i" will represent in binary which elements need to be filled.
                var fill = i;
                for (var k = 0; k < 5; k++)
                {
                    // Get whether or not the element needs to be filled
                    if ((fill & 0x01) != 0)
                    {
                        var expected = k * 32 + i + 1;
                        matrix.GetElement(new MatrixLocation(k + 1, i + 1)).Value = expected;
                    }
                    fill = (fill >> 1) & 0b011111;
                }
            }

            // Swap the two rows of interest
            matrix.SwapRows(2, 4);

            // Find the elements back
            for (var i = 0; i < 32; i++)
            {
                var fill = i;
                for (var k = 0; k < 5; k++)
                {
                    // Get the current row to test (remember we swapped them!)
                    var crow = k + 1;
                    if (crow == 2)
                        crow = 4;
                    else if (crow == 4)
                        crow = 2;

                    if ((fill & 0x01) != 0)
                    {
                        var expected = k * 32 + i + 1;
                        Assert.AreEqual(expected, matrix[crow, i + 1], 1e-12);
                    }
                    else
                        Assert.AreEqual(null, matrix.FindElement(new MatrixLocation(crow, i + 1)));
                    fill = (fill >> 1) & 0b011111;
                }
            }

            matrix.Clear();
        }

        [Test]
        public void When_SwappingColumns_Expect_Reference()
        {
            // Build the matrix
            var matrix = new SparseMatrix<double>();

            // We want to test all possible combinations for the row! We need 5 elements to be able to test it
            for (var i = 0; i < 32; i++)
            {
                // Our counter "i" will represent in binary which elements need to be filled.
                var fill = i;
                for (var k = 0; k < 5; k++)
                {
                    // Get whether or not the element needs to be filled
                    if ((fill & 0x01) != 0)
                    {
                        var expected = k * 32 + i + 1;
                        matrix.GetElement(new MatrixLocation(i + 1, k + 1)).Value = expected;
                    }
                    fill = (fill >> 1) & 0b011111;
                }
            }

            // Swap the two rows of interest
            matrix.SwapColumns(2, 4);

            // Find the elements back
            for (var i = 0; i < 32; i++)
            {
                var fill = i;
                for (var k = 0; k < 5; k++)
                {
                    // Get the current row to test (remember we swapped them!)
                    var ccolumn = k + 1;
                    if (ccolumn == 2)
                        ccolumn = 4;
                    else if (ccolumn == 4)
                        ccolumn = 2;

                    if ((fill & 0x01) != 0)
                    {
                        var expected = k * 32 + i + 1;
                        Assert.AreEqual(expected, matrix[i + 1, ccolumn], 1e-12);
                    }
                    else
                        Assert.AreEqual(null, matrix.FindElement(new MatrixLocation(i + 1, ccolumn)));
                    fill = (fill >> 1) & 0b011111;
                }
            }

            matrix.Clear();
        }

        [Test]
        [TestCaseSource(nameof(Locations3By3))]
        public void When_RemoveElement_Expect_Reference(int row, int column)
        {
            var matrix = new SparseMatrix<double>();
            var index = 1;
            for (var r = 1; r <= 3; r++)
            {
                for (var c = 1; c <= 3; c++)
                {
                    matrix.GetElement(new MatrixLocation(r, c)).Value = index;
                    index++;
                }
            }

            matrix.RemoveElement(new MatrixLocation(row, column));

            index = 1;
            for (var r = 1; r <= 3; r++)
            {
                for (var c = 1; c <= 3; c++)
                {
                    if (r == row && c == column)
                        Assert.AreEqual(null, matrix.FindElement(new MatrixLocation(r, c)));
                    else
                        Assert.AreEqual(index, matrix.FindElement(new MatrixLocation(r, c)).Value, 1e-9);
                    index++;
                }
            }
        }

        public static IEnumerable<TestCaseData> Locations3By3
        {
            get
            {
                for (var r = 1; r <= 3; r++)
                {
                    for (var c = 1; c <= 3; c++)
                        yield return new TestCaseData(r, c);
                }
            }
        }
    }
}
