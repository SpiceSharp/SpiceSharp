using System;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class WaveformTests
    {
        [Test]
        public void When_PulsedWaveform_Expect_Reference()
        {
            double v1 = -1, v2 = 3.3;
            double td = 1e-8, tr = 1e-9, tf = 2e-9, pw = 1e-8, per = 10e-8;
            var pulse = new Pulse(v1, v2, td, tr, tf, pw, per);
            pulse.Setup();

            // Simulate a few points of interest
            Assert.AreEqual(v1, pulse.At(0.0), 1e-12);
            Assert.AreEqual(v1, pulse.At(td), 1e-12);
            Assert.AreEqual(v2, pulse.At(td + tr), 1e-12);
            Assert.AreEqual(v2, pulse.At(td + tr + pw), 1e-12);
            Assert.AreEqual(v1, pulse.At(td + tr + pw + tf), 1e-12);
            Assert.AreEqual(v1, pulse.At(td + per), 1e-12);
        }

        [Test]
        public void When_PulsedWaveformSmallPulseWidth_Expect_CircuitException()
        {
            var pulse = new Pulse(0, 1, 0.5, 1, 1, 1, 0.5);
            Assert.Throws<CircuitException>(() => pulse.Setup());
        }

        [Test]
        public void When_SineWaveform_Expect_Reference()
        {
            double vo = 0.5, va = 1.8;
            var freq = 10.0;
            var sine = new Sine(vo, va, freq);
            sine.Setup();

            // Simulate a few points of interest
            var time = 0.0;
            while (time < 20.0)
            {
                var expected = vo + va * Math.Sin(freq * 2.0 * Math.PI * time);
                Assert.AreEqual(expected, sine.At(time), 1e-12);
                time += 0.1;
            }
        }
    }
}
