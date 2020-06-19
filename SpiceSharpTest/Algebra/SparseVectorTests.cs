using NUnit.Framework;
using SpiceSharp.Algebra;

namespace SpiceSharpTest.Algebra
{
    [TestFixture]
    public class SparseVectorTests
    {
        [Test]
        public void When_SwappingVectorElements_Expect_Reference()
        {
            // Test swapping of elements in all possible combinations

            for (var i = 0; i < 32; i++)
            {
                // Setup
                var vector = new SparseVector<double>(5);
                var fill = i;
                for (var k = 1; k <= 5; k++)
                {
                    if ((fill & 0x01) != 0)
                        vector[k] = k;
                    fill = (fill >> 1) & 0b011111;
                }

                // Swap rows
                vector.SwapElements(2, 4);

                // Check
                fill = i;
                for (var k = 1; k <= 5; k++)
                {
                    var realk = k;
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
