using System;
using System.Numerics;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class CurrentControlledCurrentSourceTests : Framework
    {
        [Test]
        public void When_CCCSDC_Expect_Reference()
        {
            double gain = 0.85;
            double resistance = 1e4;

            // Build the circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new CurrentSource("I1", "in", "0", 0.0),
                new VoltageSource("V1", "in", "0", 0.0),
                new CurrentControlledCurrentSource("F1", "0", "out", "V1", gain),
                new Resistor("R1", "out", "0", resistance)
                );

            // Make the simulation, exports and references
            Dc dc = new Dc("DC", "I1", -10.0, 10.0, 1e-3);
            Export<double>[] exports = { new RealVoltageExport(dc, "out"), new RealPropertyExport(dc, "R1", "i") };
            Func<double, double>[] references = { sweep => sweep * gain * resistance, sweep => sweep * gain };
            AnalyzeDC(dc, ckt, exports, references);
        }

        [Test]
        public void When_CCCSSmallSignal_Expect_Reference()
        {
            double magnitude = 0.6;
            double gain = 0.85;
            double resistance = 1e4;

            // Build the circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new CurrentSource("I1", "in", "0", 0.0),
                new VoltageSource("V1", "in", "0", 0.0),
                new CurrentControlledCurrentSource("F1", "0", "out", "V1", gain),
                new Resistor("R1", "out", "0", resistance)
                );
            ckt.Objects["I1"].ParameterSets.SetProperty("acmag", magnitude);

            // Make the simulation, exports and references
            Ac ac = new Ac("AC", new SpiceSharp.Simulations.Sweeps.DecadeSweep(1, 1e4, 3));
            Export<Complex>[] exports = { new ComplexVoltageExport(ac, "out"), new ComplexPropertyExport(ac, "R1", "i") };
            Func<double, Complex>[] references = { freq => magnitude * gain * resistance, freq => magnitude * gain };
            AnalyzeAC(ac, ckt, exports, references);
        }
    }
}
