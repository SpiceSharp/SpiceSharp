using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class SamplerTests
    {
        private IEnumerable<double> TimePoints
        {
            get
            {
                double time = 0;
                for (var i = 0; i < 100; i++)
                {
                    yield return time;
                    time += 1e-3;
                }    
            }
        }

        [Test]
        public void When_Transient_Expect_Reference()
        {
            IEnumerator<double> refPoints = TimePoints.GetEnumerator();
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Sine(0, 1, 100)),
                new Sampler("Sampler1", TimePoints, (sender, args) =>
                {
                    if (!refPoints.MoveNext())
                        throw new SpiceSharpException("Reference points already ended");
                    Assert.AreEqual(args.Time, refPoints.Current, 1e-9);
                }));
            var tran = new Transient("tran", 1e-6, 1);
            tran.Run(ckt);

            // Make sure we went through all our reference points
            Assert.IsFalse(refPoints.MoveNext());        }
    }
}
