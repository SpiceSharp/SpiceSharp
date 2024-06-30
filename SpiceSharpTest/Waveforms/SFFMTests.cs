using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpTest.Waveforms
{
    [TestFixture]
    public class SFFMTests
    {
        [Test]
        public void When_NegativeCarrierFrequency_Expect_Exception()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SFFM(0, 1, -1, 1, 1, 0, 0).Create(null));
        }

        [Test]
        public void When_NegativeSignalFrequency_Expect_Exception()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SFFM(0, 1, 1, 0, -1, 0, 0).Create(null));
        }

        [Test]
        [TestCase(1.0, 1.0, 1.0, 5.0, 0.0, 0.0, 0.0)]
        [TestCase(1.0, 10.0, 1.0, 2.5, 0.2, 2.0, 1.0)]
        public void When_SimpleTransient_Expect_Reference(double vo, double va, double fc, double mdi, double fs, double phasec, double phases)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "a", "0", new SFFM(vo, va, fc, mdi, fs, phasec, phases)));
            var tran = new Transient("tran", 1e-6, 1);
            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                double time = tran.Time;
                Assert.That(tran.GetVoltage("a"), Is.EqualTo(vo + va * Math.Sin(2.0 * Math.PI * fc * time + phasec * Math.PI / 180.0 +
                    mdi * Math.Sin(2.0 * Math.PI * fs * time + phases * Math.PI / 180.0))).Within(1e-12));
            }
        }
    }
}
