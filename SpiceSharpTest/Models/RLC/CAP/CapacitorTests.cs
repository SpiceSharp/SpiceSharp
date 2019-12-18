﻿using System;
using System.Numerics;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Components.CapacitorBehaviors;
using SpiceSharp.Diagnostics.Validation;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class CapacitorTests : Framework
    {
        [Test]
        public void When_LowpassRCOP_Expect_Reference()
        {
            /*
             * Lowpass RC circuit
             * The capacitor should act like an open circuit
             */
            var ckt = new Circuit(
                new VoltageSource("V1", "IN", "0", 1.0),
                new Resistor("R1", "IN", "OUT", 10e3),
                new Capacitor("C1", "OUT", "0", 1e-6));

            // Create simulation
            var op = new OP("op");

            // Create exports
            var exports = new IExport<double>[1];
            exports[0] = new RealVoltageExport(op, "OUT");

            // Create references
            double[] references = { 1.0 };

            // Run test
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_LowpassRCTransientTrapezoidal_Expect_Reference()
        {
            /*
             * A test for a lowpass RC circuit (DC voltage, resistor, capacitor)
             * The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.
             */
            double dcVoltage = 10;
            var resistorResistance = 10e3; // 10000;
            var capacitance = 1e-6; // 0.000001;
            var tau = resistorResistance * capacitance;

            // Build circuit
            var ckt = new Circuit(
                new Capacitor("C1", "OUT", "0", capacitance),
                new Resistor("R1", "IN", "OUT", resistorResistance),
                new VoltageSource("V1", "IN", "0", dcVoltage)
                );

            // Create simulation, exports and references
            var tran = new Transient("tran", 1e-8, 10e-6);
            tran.Configurations.GetValue<IIntegrationMethodDescription>().InitialConditions["OUT"] = 0.0;
            IExport<double>[] exports = { new RealPropertyExport(tran, "C1", "v") };
            Func<double, double>[] references = { t => dcVoltage * (1.0 - Math.Exp(-t / tau)) };

            // Run
            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);
        }

        // TODO: Fix test
        /*
        [Test]
        public void When_LowpassRCTransientGear_Expect_Reference()
        {
            // A test for a lowpass RC circuit (DC voltage, resistor, capacitor)
            // The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.
            double dcVoltage = 10;
            var resistorResistance = 10e3; // 10000;
            var capacitance = 1e-6; // 0.000001;
            var tau = resistorResistance * capacitance;

            // Build circuit
            var ckt = new Circuit(
                new Capacitor("C1", "OUT", "0", capacitance),
                new Resistor("R1", "IN", "OUT", resistorResistance),
                new VoltageSource("V1", "IN", "0", dcVoltage)
            );

            // Create simulation, exports and references
            var tran = new Transient("tran", 1e-8, 10e-6);
            var config = tran.Configurations.GetValue<IIntegrationMethodDescription>();
            config.Method = new Gear();
            config.InitialConditions["OUT"] = 0.0;
            IExport<double>[] exports = { new RealPropertyExport(tran, "C1", "v") };
            Func<double, double>[] references = { t => dcVoltage * (1.0 - Math.Exp(-t / tau)) };

            // Run
            AnalyzeTransient(tran, ckt, exports, references);
        }
        */

        [Test]
        public void When_IsTemperatureInvariant_Expect_Reference()
        {
            /*
             * A test for a lowpass RC circuit (DC voltage, resistor, capacitor)
             * The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.
             * TC1 and TC2 of capacitor is 0.
             * Temperature is 30 degrees.
             */
            double dcVoltage = 10;
            var resistorResistance = 10e3; // 10000;
            var capacitance = 1e-6; // 0.000001;
            var tau = resistorResistance * capacitance;

            var capacitor = new Capacitor("C1", "OUT", "0", capacitance);
            var model = new CapacitorModel("model C1");
            model.Parameters.GetValue<ModelBaseParameters>().TemperatureCoefficient1.Value = 0.0;
            model.Parameters.GetValue<ModelBaseParameters>().TemperatureCoefficient2.Value = 0.0;
            capacitor.Model = model.Name;

            // Build circuit
            var ckt = new Circuit(
                model,
                capacitor,
                new Resistor("R1", "IN", "OUT", resistorResistance),
                new VoltageSource("V1", "IN", "0", dcVoltage));

            // Create simulation, exports and references
            var tran = new Transient("tran", 1e-8, 10e-6);
            tran.Configurations.GetValue<IIntegrationMethodDescription>().InitialConditions["OUT"] = 0.0;

            tran.BeforeTemperature += (sender, args) =>
                {
                    ((IBiasingSimulationState)args.State).Temperature = Constants.CelsiusKelvin + 30.0;
                };

            IExport<double>[] exports = { new RealPropertyExport(tran, "C1", "v") };
            Func<double, double>[] references = { t => dcVoltage * (1.0 - Math.Exp(-t / tau)) };

            // Run
            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_IsTemperatureDependent_Expect_Reference()
        {
            /*
             * A test for a lowpass RC circuit (DC voltage, resistor, capacitor)
             * The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.
             * TC1 and TC2 of capacitor is 0.
             * Temperature is 30 degrees.
             */
            double dcVoltage = 10;
            var resistorResistance = 10e3; // 10000;
            double factor = (1.0 + 3.0 * 1.1 + 3.0 * 3.0 * 2.1);
            var capacitance = 1e-6;
            var capacitanceAfterTemperature = capacitance * factor;

            var tau = resistorResistance * capacitanceAfterTemperature;

            var capacitor = new Capacitor("C1", "OUT", "0", capacitance);
            var model = new CapacitorModel("model C1");
            model.Parameters.GetValue<ModelBaseParameters>().TemperatureCoefficient1.Value = 1.1;
            model.Parameters.GetValue<ModelBaseParameters>().TemperatureCoefficient2.Value = 2.1;
            capacitor.Model = model.Name;

            // Build circuit
            var ckt = new Circuit(
                model,
                capacitor,
                new Resistor("R1", "IN", "OUT", resistorResistance),
                new VoltageSource("V1", "IN", "0", dcVoltage));

            // Create simulation, exports and references
            var tran = new Transient("tran", 1e-8, 10e-6);
            tran.Configurations.GetValue<IIntegrationMethodDescription>().InitialConditions["OUT"] = 0.0;

            tran.BeforeTemperature += (sender, args) =>
                {
                    ((IBiasingSimulationState)args.State).Temperature = Constants.CelsiusKelvin + 30.0;
                };

            IExport<double>[] exports = { new RealPropertyExport(tran, "C1", "v") };
            Func<double, double>[] references = { t => dcVoltage * (1.0 - Math.Exp(-t / tau)) };

            // Run
            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_LowpassRCSmallSignal_Expect_Reference()
        {
            /*
             * Lowpass RC filter in the frequency domain should have a single pole at s=-2pi*R*C
             */
            // Create circuit
            var resistance = 1e3;
            var capacitance = 1e-6;
            var ckt = new Circuit(
                new VoltageSource("V1", "IN", "0", 0.0)
                    .SetParameter("acmag", 1.0),
                new Resistor("R1", "IN", "OUT", resistance),
                new Capacitor("C1", "OUT", "0", capacitance)
                );

            // Create simulation
            var ac = new AC("ac", new DecadeSweep(0.1, 1.0e6, 10));

            // Create exports
            IExport<Complex>[] exports = { new ComplexPropertyExport(ac, "C1", "v") };

            // Create references
            Func<double, Complex>[] references = { f => 1.0 / new Complex(1.0, resistance * capacitance * 2 * Math.PI * f) };

            // Run test
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_MultipliersTransient_Expect_Reference()
        {
            // Build circuit
            var ckt_actual = new Circuit(
                new Capacitor("C1", "out", "0", 1e-6).SetParameter("m", 3.0),
                new Resistor("R1", "in", "out", 1e3),
                new VoltageSource("V1", "in", "0", 1.0)
                );
            var ckt_reference = new Circuit(
                new Resistor("R1", "in", "out", 1e3),
                new VoltageSource("V1", "in", "0", 1.0)
                );
            ParallelSeries(ckt_reference, name => new Capacitor(name, "", "", 1e-6), "in", "0", 3, 1);

            // Create simulation, exports and references
            var tran = new Transient("tran", 1e-8, 10e-6);
            tran.Configurations.GetValue<IIntegrationMethodDescription>().InitialConditions["OUT"] = 0.0;
            IExport<double>[] exports = { new RealVoltageExport(tran, "out") };

            // Run 
            Compare(tran, ckt_reference, ckt_actual, exports);
            DestroyExports(exports);
        }

        [Test]
        public void When_MultipliersSmallSignal_Expect_Reference()
        {
            // Build circuit
            var ckt_actual = new Circuit(
                new Capacitor("C1", "out", "0", 1e-6).SetParameter("m", 3.0),
                new Resistor("R1", "in", "out", 1e3),
                new VoltageSource("V1", "in", "0", 1.0)
                );
            var ckt_reference = new Circuit(
                new Resistor("R1", "in", "out", 1e3),
                new VoltageSource("V1", "in", "0", 1.0)
                );
            ParallelSeries(ckt_reference, name => new Capacitor(name, "", "", 1e-6), "in", "0", 3, 1);

            // Create simulation, exports and references
            var ac = new AC("ac", new DecadeSweep(1, 1e6, 3));
            IExport<Complex>[] exports = { new ComplexVoltageExport(ac, "out") };

            // Run 
            Compare(ac, ckt_reference, ckt_actual, exports);
            DestroyExports(exports);
        }

        [Test]
        public void When_DecoupledNodeValidation_Expect_FloatingNodeException()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0),
                new Capacitor("C1", "in", "out", 1e-6));
            Assert.Throws<FloatingNodeException>(() => ckt.Validate());
        }

        [Test]
        public void When_ShortedValidation_Expect_ShortCircuitComponentException()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0),
                new Capacitor("C1", "in", "in", 1e-5));
            Assert.Throws<ShortCircuitComponentException>(() => ckt.Validate());
        }
    }
}
