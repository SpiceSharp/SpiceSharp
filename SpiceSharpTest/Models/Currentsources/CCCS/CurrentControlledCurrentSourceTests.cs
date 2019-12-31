﻿using System;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class CurrentControlledCurrentSourceTests : Framework
    {
        [Test]
        public void When_SimpleDC_Expect_Reference()
        {
            var gain = 0.85;
            var resistance = 1e4;

            // Build the circuit
            var ckt = new Circuit(
                new CurrentSource("I1", "0", "in", 0.0),
                new VoltageSource("V1", "in", "0", 0.0),
                new CurrentControlledCurrentSource("F1", "0", "out", "V1", gain),
                new Resistor("R1", "out", "0", resistance)
                );

            // Make the simulation, exports and references
            var dc = new DC("DC", "I1", -10.0, 10.0, 1e-3);
            IExport<double>[] exports = { new RealVoltageExport(dc, "out"), new RealPropertyExport(dc, "R1", "i") };
            Func<double, double>[] references = { sweep => sweep * gain * resistance, sweep => sweep * gain };
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleSmallSignal_Expect_Reference()
        {
            var magnitude = 0.6;
            var gain = 0.85;
            var resistance = 1e4;

            // Build the circuit
            var ckt = new Circuit(
                new CurrentSource("I1", "0", "in", 0.0)
                    .SetParameter("acmag", magnitude),
                new VoltageSource("V1", "in", "0", 0.0),
                new CurrentControlledCurrentSource("F1", "0", "out", "V1", gain),
                new Resistor("R1", "out", "0", resistance)
                );

            // Make the simulation, exports and references
            var ac = new AC("AC", new DecadeSweep(1, 1e4, 3));
            IExport<Complex>[] exports = { new ComplexVoltageExport(ac, "out"), new ComplexPropertyExport(ac, "R1", "i_c") };
            Func<double, Complex>[] references = { freq => magnitude * gain * resistance, freq => magnitude * gain };
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_FloatingOutput_Expect_SimulationValidationFailedException()
        {
            var ckt = new Circuit(
                new CurrentSource("I1", "0", "in", 0),
                new VoltageSource("V1", "in", "0", 0),
                new CurrentControlledCurrentSource("F1", "out", "0", "V1", 12.0)
                );

            // Make the simulation and run it
            var op = new OP("op");
            var ex = Assert.Throws<SimulationValidationFailed>(() => op.Run(ckt));
            Assert.AreEqual(1, ex.Rules.ViolationCount);
            // TODO: Check more
        }
    }
}
