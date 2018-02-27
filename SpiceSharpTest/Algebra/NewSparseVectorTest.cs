using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Algebra;

namespace SpiceSharpTest.Sparse
{
    [TestClass]
    public class NewSparseVectorTest
    {
        [TestMethod]
        public void TestSwap()
        {
            // Test swapping of elements in all possible combinations

            for (int i = 0; i < 32; i++)
            {
                // Setup
                SparseVector<double> vector = new SparseVector<double>(5);
                int fill = i;
                for (int k = 1; k <= 5; k++)
                {
                    if ((fill & 0x01) != 0)
                        vector[k] = k;
                    fill = (fill >> 1) & 0b011111;
                }

                // Swap rows
                vector.Swap(2, 4);

                // Check
                fill = i;
                for (int k = 1; k <= 5; k++)
                {
                    int realk = k;
                    if (k == 2)
                        realk = 4;
                    else if (k == 4)
                        realk = 2;

                    if ((fill & 0x01) != 0)
                        Assert.AreEqual(k, vector[realk], 1e-12);
                    else
                        Assert.AreEqual(vector.FindElement(realk), null);
                    fill = (fill >> 1) & 0b011111;
                }
            }
        }
    }
}
