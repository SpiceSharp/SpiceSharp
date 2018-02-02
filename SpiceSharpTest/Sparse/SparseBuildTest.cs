using System;
using System.Collections.Generic;
using System.Numerics;
using SpiceSharp.Sparse;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SpiceSharpTest.Sparse
{
    [TestClass]
    public class SparseBuildTest
    {
        /// <summary>
        /// Random 3x3 matrix
        /// </summary>
        [TestMethod]
        public void TestBasic1()
        {
            Matrix<double> matrix = new Matrix<double>();
            matrix.GetElement(1, 1).Value = 1.0;
            matrix.GetElement(1, 2).Value = 2.0;
            matrix.GetElement(3, 3).Value = 3.0;
            matrix.GetElement(2, 3).Value = 4.0;
            matrix.GetElement(3, 1).Value = 5.0;

            Assert.AreEqual(1.0, matrix.FindReorderedElement(1, 1).Element.Value);
            Assert.AreEqual(2.0, matrix.FindReorderedElement(1, 2).Element.Value);
            Assert.AreEqual(null, matrix.FindReorderedElement(1, 3));
            Assert.AreEqual(null, matrix.FindReorderedElement(2, 1));
            Assert.AreEqual(null, matrix.FindReorderedElement(2, 2));
            Assert.AreEqual(4.0, matrix.FindReorderedElement(2, 3).Element.Value);
            Assert.AreEqual(5.0, matrix.FindReorderedElement(3, 1).Element.Value);
            Assert.AreEqual(null, matrix.FindReorderedElement(3, 2));
            Assert.AreEqual(3.0, matrix.FindReorderedElement(3, 3).Element.Value);
        }

        /// <summary>
        /// Random matrix for growing
        /// </summary>
        [TestMethod]
        public void TestBasic2()
        {
            Matrix<Complex> matrix = new Matrix<Complex>();
            for (int r = 1; r < 100; r++)
            {
                for (int c = 1; c < 100; c++)
                {
                    matrix.GetElement(r, c).Value = new Complex(r, c);
                }
            }

            for (int r = 1; r < 100; r++)
            {
                for (int c = 1; c < 100; c++)
                {
                    Assert.AreEqual(new Complex(r, c), matrix.FindReorderedElement(r, c).Element.Value);
                }
            }
        }

        /// <summary>
        /// Test matrix element links after building a big matrix
        /// </summary>
        [TestMethod]
        public void TestLinks1()
        {
            // Test row links
            Matrix<Complex> matrix = new Matrix<Complex>();
            MatrixElement<Complex> elt = null;
            
            for (int r = 1; r < 100; r++)
            {
                bool oddrow = r % 2 == 0;
                for (int c = 1; c < 100; c++)
                {
                    bool oddcol = c % 2 == 0;

                    if (oddrow && !oddcol || !oddrow && oddcol)
                        matrix.GetElement(r, c).Value = new Complex(r, c);
                }
            }
            matrix.LinkRows();

            // Please note that the elements are reordered internally!
            for (int r = 1; r < 100; r++)
            {
                // Test row links
                elt = matrix.FirstInRow[matrix.Translation.ExtToIntRowMap[r]];
                bool oddrow = r % 2 == 0;

                for (int c = 1; c < 100; c++)
                {
                    bool oddcol = c % 2 == 0;

                    if (oddrow && !oddcol || !oddrow && oddcol)
                    {
                        Assert.AreEqual(elt.Element.Value, new Complex(r, c));
                        elt = elt.NextInRow;
                    }
                }
            }
            for (int c = 1; c < 100; c++)
            {
                // Test column links
                elt = matrix.FirstInCol[matrix.Translation.ExtToIntColMap[c]];
                bool oddcol = c % 2 == 0;

                for (int r = 1; r < 100; r++)
                {
                    bool oddrow = r % 2 == 0;

                    if (oddrow && !oddcol || !oddrow && oddcol)
                    {
                        Assert.AreEqual(elt.Element.Value, new Complex(r, c));
                        elt = elt.NextInColumn;
                    }
                }
            }
        }
    }
}
