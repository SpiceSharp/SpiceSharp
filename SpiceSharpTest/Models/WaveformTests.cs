using NUnit.Framework;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class WaveformTests
    {
        /*
        [Test]
        public void When_PulsedWaveform_Expect_Reference()
        {
            double v1 = -1, v2 = 3.3;
            double td = 1e-8, tr = 1e-9, tf = 2e-9, pw = 1e-8, per = 10e-8;
            var pulse = new Pulse(v1, v2, td, tr, tf, pw, per);
            pulse.Setup();

            // Simulate a few points of interest
            var simulation = Substitute.For<TimeSimulationMock>("mock");
            simulation.Setup();
            simulation.Method.Time.Returns(0.0, td, td + tr, td + tr + pw, td + tr + pw + tf, td + per);
            double[] reference = {v1, v1, v2, v2, v1, v1};

            for (var i = 0; i < 6; i++)
            {
                pulse.Probe(simulation);
                Assert.AreEqual(reference[i], pulse.Value, 1e-12);
            }
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
            var simulation = Substitute.For<TimeSimulationMock>("mock");
            simulation.Setup();
            var time = 0.0;
            while (time < 20.0)
            {
                var expected = vo + va * Math.Sin(freq * 2.0 * Math.PI * time);
                simulation.Method.Time.Returns(time);
                sine.Probe(simulation);
                Assert.AreEqual(expected, sine.Value, 1e-12);
                time += 0.1;
            }
        }
        */
    }
}
