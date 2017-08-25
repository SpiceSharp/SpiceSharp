using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Simulations;
using SpiceSharp.Diagnostics;

namespace SpiceSharpTest.Parser
{
    [TestClass]
    public class SpiceSharpParserSimulationTest : Framework
    {
        [TestMethod]
        public void ACTest()
        {
            var netlist = Run(".ac dec 100 1 10g",
                ".ac lin 1024 0 1k",
                ".ac oct 8 1 10.5megHz");

            TestParameters(netlist.Simulations[0], new string[] { "n", "start", "stop" }, new double[] { 100, 1, 10e9 });
            Assert.AreEqual(((AC)netlist.Simulations[0]).StepType, AC.StepTypes.Decade);
            TestParameters(netlist.Simulations[1], new string[] { "n", "start", "stop" }, new double[] { 1024, 0, 1e3 });
            Assert.AreEqual(((AC)netlist.Simulations[1]).StepType, AC.StepTypes.Linear);
            Assert.AreEqual(((AC)netlist.Simulations[2]).StepType, AC.StepTypes.Octave);

            if (CircuitWarning.Warnings.Count > 0)
                throw new Exception("Warning: " + CircuitWarning.Warnings[0]);
        }

        [TestMethod]
        public void DCTest()
        {
            var netlist = Run(".dc V1 0 5 1m");
            TestParameters(((DC)netlist.Simulations[0]).Sweeps[0], new string[] { "start", "stop", "step" }, new double[] { 0.0, 5.0, 1e-3 });
            Assert.AreEqual(((DC)netlist.Simulations[0]).Sweeps[0].ComponentName, "v1");

            if (CircuitWarning.Warnings.Count > 0)
                throw new Exception("Warning: " + CircuitWarning.Warnings[0]);
        }

        [TestMethod]
        public void TransientTest()
        {
            var netlist = Run(".tran 1p 10n", ".tran 1n 2u 1u 10n");
            TestParameters(netlist.Simulations[0], new string[] { "step", "stop" }, new double[] { 1e-12, 10e-9 });
            TestParameters(netlist.Simulations[1], new string[] { "step", "stop", "start", "maxstep" }, new double[] { 1e-9, 2e-6, 1e-6, 10e-9 });

            if (CircuitWarning.Warnings.Count > 0)
                throw new Exception("Warning: " + CircuitWarning.Warnings[0]);
        }
    }
}
