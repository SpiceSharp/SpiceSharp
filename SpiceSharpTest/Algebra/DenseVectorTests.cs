using NUnit.Framework;
using SpiceSharp.Algebra;

namespace SpiceSharpTest.Algebra
{
    [TestFixture]
    public class DenseVectorTests
    {
        [Test]
        public void When_SwappingVectorElements_Expect_Reference()
        {
            var vector = new DenseVector<double>(5);
            for (int k = 1; k <= 5; k++)
                vector[k] = k;

            vector.SwapElements(2, 4);

            for (int k = 1; k <= 5; k++)
                Assert.That(vector[k], Is.EqualTo(k == 2 ? 4 : k == 4 ? 2 : k).Within(1e-12));
        }
    }
}
