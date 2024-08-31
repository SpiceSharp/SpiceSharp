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

            for (int i = 0; i < 32; i++)
            {
                // Setup
                var vector = new SparseVector<double>(5);
                int fill = i;
                for (int k = 1; k <= 5; k++)
                {
                    if ((fill & 0x01) != 0)
                        vector[k] = k;
                    fill = (fill >> 1) & 0b011111;
                }

                // Swap rows
                vector.SwapElements(2, 4);

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
                        Assert.That(vector[realk], Is.EqualTo(k).Within(1e-12));
                    else
                        Assert.That(vector.FindElement(realk), Is.Null);
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

            for (int i = 1; i <= 3; i++)
            {
                if (i == index)
                    Assert.That(vector.FindElement(i), Is.EqualTo(null));
                else
                    Assert.That(vector.FindElement(i).Value, Is.EqualTo(i));
            }
        }
    }
}
