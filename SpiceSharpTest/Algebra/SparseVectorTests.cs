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

        [Test]
        [TestCase(1), TestCase(2), TestCase(3)]
        public void When_RemoveElement_Expect_Reference(int index)
        {
            var vector = new SparseVector<double>();
            vector.GetElement(1).Value = 1;
            vector.GetElement(2).Value = 2;
            vector.GetElement(3).Value = 3;

            vector.RemoveElement(index);

            for (var i = 1; i <= 3; i++)
            {
                if (i == index)
                    Assert.AreEqual(null, vector.FindElement(i));
                else
                    Assert.AreEqual(i, vector.FindElement(i).Value);
            }
        }
    }
}
