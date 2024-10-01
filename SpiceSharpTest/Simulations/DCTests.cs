using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpTest.Models;
using System;
using System.Collections.Generic;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class DCTests : Framework
    {
        private Diode CreateDiode(string name, string anode, string cathode, string model)
        {
            var d = new Diode(name) { Model = model };
            d.Connect(anode, cathode);
            return d;
        }

        private DiodeModel CreateDiodeModel(string name, string parameters)
        {
            var model = new DiodeModel(name);
            ApplyParameters(model, parameters);
            return model;
        }

        [Test]
        public void When_DCSweepResistorParameter_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0),
                new Resistor("R1", "in", "out", 1.0e4),
                new Resistor("R2", "out", "0", 1.0e4)
                );

            // Do a DC sweep where one of the sweeps is a parameter
            var dc = new DC("DC 1");
            dc.DCParameters.Sweeps.Add(new ParameterSweep("R2", "resistance", new LinearSweep(0.0, 1e4, 1e3), container =>
            {
                container.GetValue<ITemperatureBehavior>().Temperature();
            })); // Sweep R2 from 0 to 10k per 1k
            dc.DCParameters.Sweeps.Add(new ParameterSweep("V1", new LinearSweep(0, 5, 0.1))); // Sweep V1 from 0V to 5V per 100mV

            // Run simulation
            foreach (int _ in dc.Run(ckt))
            {
                double resistance = Math.Max(dc.GetCurrentSweepValue()[0], SpiceSharp.Components.Resistors.Parameters.MinimumResistance);
                double voltage = dc.GetCurrentSweepValue()[1];
                double expected = voltage * resistance / (resistance + 1.0e4);
                Assert.That(dc.GetVoltage("out"), Is.EqualTo(expected).Within(1e-12));
            }
        }

        [Test]
        public void When_DiodeDCTwice_Expect_NoException()
        {
            /*
             * Bug found by Marcin Golebiowski
             * Running simulations twice will give rise to errors. We are using a diode model here
             * in order to make sure we're use states, extra equations, etc.
             */

            var ckt = new Circuit
            {
                CreateDiode("D1", "OUT", "0", "1N914"),
                CreateDiodeModel("1N914", "Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9"),
                new VoltageSource("V1", "OUT", "0", 0.0)
            };

            // Create simulations
            var dc = new DC("DC 1", "V1", -1, 1, 10e-3);
            var op = new OP("OP 1");

            // Create exports
            var dcExportV1 = new RealPropertyExport(dc, "V1", "i");
            var dcExportV12 = new RealPropertyExport(dc, "V1", "i");
            var opExportV1 = new RealPropertyExport(op, "V1", "i");

            // Run DC and op
            foreach (int _ in dc.Run(ckt))
            {
                double v1 = dcExportV1.Value;
                double v12 = dcExportV12.Value;
            }
            foreach (int _ in dc.Run(ckt))
            {
                double v1 = dcExportV1.Value;
                double v12 = dcExportV12.Value;
            }
            foreach (int _ in op.Run(ckt))
            {
                double v1 = opExportV1.Value;
            }
        }

        [Test]
        public void When_DiodeDCRerun_Expect_Same()
        {
            var ckt = new Circuit
            {
                CreateDiode("D1", "OUT", "0", "1N914"),
                CreateDiodeModel("1N914", "Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9"),
                new VoltageSource("V1", "OUT", "0", 0.0)
            };

            // Create simulations
            var dc = new DC("DC 1", "V1", -1, 1, 10e-3);

            // Create exports
            var dcExportV1 = new RealPropertyExport(dc, "V1", "i");

            // First run: build the reference
            var r = new List<double>();
            foreach (int _ in dc.Run(ckt))
                r.Add(dcExportV1.Value);

            // Rerun: check with reference
            int index = 0;
            foreach (int _ in dc.Rerun())
                Assert.That(r[index++], Is.EqualTo(dcExportV1.Value).Within(1e-20));
        }

        [Test]
        public void When_MultipleDC_Expect_Reference()
        {
            /*
             * We test if the simulation can run twice on different circuits with
             * different number of equations.
             */
            var cktA = new Circuit(
                new VoltageSource("V1", "in", "0", 0),
                new Resistor("R1", "in", "out", 1e3),
                new Resistor("R2", "out", "0", 1e3));
            var cktB = new Circuit(
                new VoltageSource("V1", "in", "0", 0),
                new Resistor("R1", "in", "out", 1e3),
                new Resistor("R2", "out", "int", 1e3),
                new Resistor("R3", "int", "0", 1e3));

            var dc = new DC("dc", "V1", -2, 2, 0.1);
            foreach (int _ in dc.Run(cktB))
                Assert.That(dc.GetVoltage("out"), Is.EqualTo(dc.GetVoltage("in") * 2.0 / 3.0).Within(1e-12));
            foreach (int _ in dc.Run(cktA))
                Assert.That(dc.GetVoltage("out"), Is.EqualTo(dc.GetVoltage("in") * 0.5).Within(1e-12));
        }

        [Test]
        public void When_DCRun_Expect_YieldFlags()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0),
                new Resistor("R1", "in", "out", 1.0e4),
                new Resistor("R2", "out", "0", 1.0e4)
                );

            // Create the simulation
            var dc = new DC("DC 1", [new ParameterSweep("R2", "resistance", new LinearSweep(0.0, 1e4, 1e3), container =>
            {
                container.GetValue<ITemperatureBehavior>().Temperature();
            })]);

            int flags = 0;
            foreach (int flag in dc.Run(ckt, mask: -1))
                flags |= flag;

            Assert.That(flags, Is.EqualTo(
                Simulation.BeforeSetup |
                Simulation.AfterSetup |
                Simulation.BeforeValidation |
                Simulation.AfterValidation |
                Simulation.BeforeExecute |
                Simulation.AfterExecute |
                Simulation.BeforeUnsetup |
                Simulation.AfterUnsetup |
                BiasingSimulation.BeforeTemperature |
                BiasingSimulation.AfterTemperature |
                DC.ExportSweep));
        }
    }
}
