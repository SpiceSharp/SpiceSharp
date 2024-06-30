using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpTest.Waveforms
{
    [TestFixture]
    public class AMTests
    {
        [Test]
        public void When_NegativeModulationFrequency_Expect_Exception()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AM(1, 0, -1, 1, 0, 0, 0).Create(null));
        }

        [Test]
        public void When_NegativeCarrierFrequency_Expect_Exception()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AM(1, 0, 1, -1, 0, 0, 0).Create(null));
        }

        [Test]
        [TestCase(1.0, 0.0, 1.0, 5.0, 0.0, 0.0, 0.0)]
        [TestCase(1.0, -1.0, 1.0, 2.5, 0.2, 2.0, 1.0)]
        public void When_SimpleTransient_Expect_Reference(double va, double vo, double mf, double fc, double td, double phasec, double phases)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "a", "0", new AM(va, vo, mf, fc, td, phasec, phases)));
            var tran = new Transient("tran", 1e-6, 1);
            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                double time = tran.Time - td;
                if (time <= 0)
                    Assert.That(tran.GetVoltage("a"), Is.EqualTo(0.0).Within(1e-12));
                else
                {
                    Assert.That(
                        tran.GetVoltage("a"), Is.EqualTo(va * (vo + Math.Sin(2.0 * Math.PI * mf * time + phases * Math.PI / 180.0)) *
                        Math.Sin(2.0 * Math.PI * fc * time + phasec * Math.PI / 180.0)).Within(1e-12));
                }
            };
            tran.Run(ckt);
        }
    }
}
