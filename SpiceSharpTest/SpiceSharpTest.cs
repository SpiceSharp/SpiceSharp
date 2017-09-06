using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Diagnostics;

namespace SpiceSharpTest
{
    [TestClass]
    public class SpiceSharpTest
    {
        /// <summary>
        /// Relative tolerance for things that will be accurate
        /// </summary>
        private double Tolerance = 1e-15;

        /// <summary>
        /// Absolute tolerance for transient analysis
        /// Solving differential equations will accumulate errors, so
        /// we cannot use a very strict tolerance
        /// </summary>
        private double TranTolerance = 1e-3;

        [TestMethod]
        public void TestAC()
        {
            Circuit ckt = new Circuit();

            // Add driving voltage source
            Voltagesource vsrc = new Voltagesource("V1");
            vsrc.Connect("IN", "GND");
            vsrc.Set("dc", 1.0);
            vsrc.Set("acmag", 1.0);
            vsrc.Set("acphase", 0.0);

            // Add resistor
            Resistor res = new Resistor("R1");
            res.Connect("IN", "OUT");
            res.Set("resistance", 1.0e3);

            // Add capacitor
            Capacitor cap = new Capacitor("C1");
            cap.Connect("OUT", "GND");
            cap.Set("capacitance", 1.0e-6);

            // Build the circuit
            ckt.Objects.Add(vsrc, res, cap);

            // Create the simulation
            AC ac = new AC("TestAC");
            ac.StartFreq = 1.0;
            ac.StopFreq = 10e9;
            ac.Steps = 100;
            ac.StepType = AC.StepTypes.Decade;

            // Perform the simulation
            ac.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                Complex laplace = data.Circuit.State.Complex.Laplace;
                Complex output = data.GetPhasor("OUT");

                // Calculate the theoretical value
                Complex expected = 1.0 / (1.0 + 1.0e3 * 1.0e-6 * laplace);
                Assert.AreEqual(Error(output.Real, expected.Real), 0.0, Tolerance);
                Assert.AreEqual(Error(output.Imaginary, expected.Imaginary), 0.0, Tolerance);
            };
            ckt.Simulate(ac);

            // No warnings allowed
            Assert.AreEqual(CircuitWarning.Warnings.Count, 0, "No warnings allowed.");
        }

        [TestMethod]
        public void TestDC()
        {
            Circuit ckt = new Circuit();

            // Create a simple resistive divider
            Voltagesource vsrc = new Voltagesource("V1", "IN", "GND", 0.0);
            Resistor res1 = new Resistor("R1", "IN", "OUT", 1.0e3);
            Resistor res2 = new Resistor("R2", "OUT", "0", 2.0e3);
            Capacitor cap = new Capacitor("C1", "OUT", "GND", 1.0e-5); // Should not do anything

            // Add to the circuit
            ckt.Objects.Add(vsrc, res1, res2);

            // Create the simulation
            DC dc = new DC("TestDC");
            DC.Sweep sweep = new DC.Sweep("V1", 0.0, 10.0, 1.0e-3);
            dc.Sweeps.Add(sweep);
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double input = sweep.CurrentValue;
                double output = data.GetVoltage("OUT");
                double expected = input * 2.0 / 3.0;

                Assert.AreEqual(Error(output, expected), 0.0, Tolerance);
            };
            ckt.Simulate(dc);

            // No warnings allowed
            Assert.AreEqual(CircuitWarning.Warnings.Count, 0, "No warnings allowed.");
        }

        [TestMethod]
        public void TestTransient()
        {
            Circuit ckt = new Circuit();

            // Create a simple lowpass network
            Voltagesource vsrc = new Voltagesource("V1", "IN", "GND", 
                new Pulse(0.0, 5.0, 1e-3, 1e-9, 1e-9, 10, 20));
            vsrc.Set("dc", 0.0);
            Resistor res = new Resistor("R1", "IN", "OUT", 1e3);
            Capacitor cap = new Capacitor("C1", "OUT", "GND", 1e-6);
            ckt.Objects.Add(vsrc, res, cap);

            // Create the simulation
            Transient tran = new Transient("TestTransient");
            tran.Set("stop", 5e-3);
            tran.Set("step", 1e-9);
            double max = 0.0;
            double timemax = 0.0;
            tran.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double time = data.GetTime();
                double output = data.GetVoltage("OUT");
                double expected = 0.0;
                if (time > 1e-3)
                    expected = 5.0 * (1.0 - Math.Exp(-(time - 1e-3) / 1e-3));

                double error = Math.Abs(output - expected);
                if (error > max)
                {
                    timemax = time;
                    max = error;
                }
            };
            ckt.Simulate(tran);
            Assert.AreEqual(max, 0.0, TranTolerance); // Within 1mV precision seems a bit high no?

            // No warnings allowed
            Assert.AreEqual(CircuitWarning.Warnings.Count, 0, "No warnings allowed.");
        }

        /// <summary>
        /// Calculate the error
        /// </summary>
        /// <param name="check">The value to check</param>
        /// <param name="expected">The expected value</param>
        /// <returns></returns>
        private double Error(double check, double expected)
        {
            double error = check - expected;
            if (expected != 0.0)
                error /= expected;
            return error;
        }
    }
}
