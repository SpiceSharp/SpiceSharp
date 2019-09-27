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
                new ParallelLoader("P1", sub).SetParameter(typeof(SpiceSharp.Behaviors.IBiasingBehavior)));

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
            var statistics = ac.Statistics.GetValue<BiasingSimulationStatistics>();
            Console.WriteLine(statistics.LoadTime.Elapsed.ToString());
            statistics.Reset();

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
            Console.WriteLine(statistics.LoadTime.Elapsed.ToString());
        }

        [Test]
        public void When_CascadedOTA_Expect_Reference()
        {
            var nmosModel = new Mosfet1Model("MM1")
                .SetParameter("is", 1e-20)
                .SetParameter("vto", 0.5)
                .SetParameter("lambda", 1e-4)
                .SetParameter("kp", 5e-4);
            var ota = new Circuit(
                new VoltageSource("Vsupply", "vdd", "0", 5),
                new Mosfet1("M1", "outn", "inp", "common", "0", "MM1"),
                new Mosfet1("M2", "outp", "inn", "common", "0", "MM1"),
                new CurrentSource("Ibias", "common", "0", 100e-6),
                new Resistor("R1", "vdd", "outp", 10e3),
                new Resistor("R2", "vdd", "outn", 10e3)
                );

            var cascade = new Circuit();
            string inpp = "inpp", inpn = "inpn";
            for (var n = 1; n <= 5; n++)
            {
                string outpp = "inpp" + n;
                string outpn = "inpn" + n;
                cascade.Add(new Subcircuit("X" + n, ota, "inp", "inn", "outp", "outn").Connect(inpp, inpn, outpp, outpn));
            }

            var ckt = new Circuit(
                nmosModel,
                new VoltageSource("Vref", "inpp", "0", 2.5),
                new VoltageSource("Vs", "inpn", "0", 2.5),
                new ParallelLoader("P1", cascade).SetParameter(typeof(SpiceSharp.Behaviors.IBiasingBehavior))
                );

            var dc = new DC("dc", "Vs", 0, 5, 0.0001);
            dc.Run(ckt);

            // Show statistics
            Console.WriteLine(dc.Statistics.GetValue<BiasingSimulationStatistics>().LoadTime.Elapsed.ToString());
        }
    }
}
