using NUnit.Framework;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class SubcircuitTests : Framework
    {
        [Test]
        public void When_SubcircuitSimple_Expect_Reference()
        {
            var solver = new RealSolver();
            var collection = new SolverSet();
            collection.Add(solver);
        }
    }
}
