using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharpTest.ParallelLoaders
{
    [TestFixture]
    public class ParallelLoaderTests
    {
        [Test]
        public void When_ParallelRC_Expect_Reference()
        {
            /*
             * It doesn't really make sense to put resistors and capacitors inside a parallel loader,
             * their loading methods are incredibly short/cheap. Multithreading would only become more
             * interesting for longer loading methods, such as the BSIM models.
             */
            int count = 1000;
            var sub = new Circuit();
            string input = "in";
            for (var i = 1; i <= count; i++)
            {
                string output = "out" + i;
                sub.Add(new Resistor("R" + i, input, output, 0.5));
                sub.Add(new Capacitor("C" + i, output, "0", 1e-6));
                input = output;
            }
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0).SetParameter("acmag", 1.0),
                new ParallelLoader("P1", sub).SetParameter(typeof(SpiceSharp.Behaviors.IFrequencyBehavior))
                );

            // Build the simulation
            var ac = new AC("ac", new LinearSweep(0, 1, 3));

            // Track the current values
            Complex[] current = new Complex[3];
            int index = 0;
            void TrackCurrent(object sender, ExportDataEventArgs args)
            {
                current[index++] = args.GetComplexVoltage("out" + count);
            }
            ac.ExportSimulationData += TrackCurrent;
            ac.Run(ckt);
            ac.ExportSimulationData -= TrackCurrent;

            // Compare to a reference (flattened version)
            ckt.Remove("P1");
            ckt.Merge(sub);
            index = 0;
            void CompareReference(object sender, ExportDataEventArgs args)
            {
                var expected = args.GetComplexVoltage("out" + count);
                Assert.AreEqual(expected.Real, current[index].Real, 1e-12);
                Assert.AreEqual(expected.Imaginary, current[index].Imaginary, 1e-12);
                index++;
            }
            ac.ExportSimulationData += CompareReference;
            ac.Run(ckt);
        }

        [Test]
        public void When_ParallelInverter_Expect_NoException()
        {
            var model = new Mosfet1Model("MM1")
                .SetParameter("kp", 1e-3)
                .SetParameter("vto", 0.5);
            var p = new Circuit();
            for (var i = 0; i < 512; i++)
            {
                var m = new Mosfet1("M" + i, "out" + i, "in", "0", "0", "MM1");
                m.LinkParameters = false;
                p.Add(m);
            }
            var ckt = new Circuit(
                new VoltageSource("Vin", "in", "0", 0.0),
                new VoltageSource("Vdd", "vdd", "0", 5.0),
                model,
                new ParallelLoader("P1", p).SetParameter(typeof(SpiceSharp.Behaviors.IBiasingBehavior))
                );
            for (var i = 0; i < 512; i++)
            {
                ckt.Add(new Resistor("R" + i, "out" + i, "vdd", 1e3));
            }

            var dc = new DC("dc", "Vin", 0, 5, 1e-3);
            dc.Run(ckt);
        }
    }
}
