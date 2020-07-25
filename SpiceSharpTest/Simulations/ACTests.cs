using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class ACTests
    {
        [Test]
        public void When_ACRerun_Expect_Same()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0).SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "out", 10),
                new Capacitor("C1", "out", "0", 20)
            );

            // Create the transient analysis
            var ac = new AC("ac 1", new DecadeSweep(1, 1e9, 10));
            var export = new ComplexVoltageExport(ac, "out");

            // Run the simulation a first time for building the reference values
            var r = new List<Complex>();
            void BuildReference(object sender, ExportDataEventArgs args) => r.Add(export.Value);
            ac.ExportSimulationData += BuildReference;
            ac.Run(ckt);
            ac.ExportSimulationData -= BuildReference;

            // Rerun the simulation for building the reference values
            var index = 0;
            void CheckReference(object sender, ExportDataEventArgs args)
            {
                Assert.AreEqual(r[index].Real, export.Value.Real, 1e-20);
                Assert.AreEqual(r[index++].Imaginary, export.Value.Imaginary, 1e-20);
            }
            ac.ExportSimulationData += CheckReference;
            ac.Rerun();
            ac.ExportSimulationData -= CheckReference;
        }
    }
}
