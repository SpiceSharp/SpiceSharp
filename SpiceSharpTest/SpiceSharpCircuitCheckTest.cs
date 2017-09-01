using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;

namespace SpiceSharpTest
{
    [TestClass]
    public class SpiceSharpCircuitCheckTest
    {
        [TestMethod]
        [ExpectedException(typeof(CircuitException), "Voltage source loop has been allowed")]
        public void TestVoltageloop1()
        {
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "A", "GND", 1.0),
                new Voltagesource("V2", "B", "A", 1.0),
                new Voltagesource("V3", "B", "A", 1.0)
                );

            CircuitCheck check = new CircuitCheck();
            check.Check(ckt);
        }

        [TestMethod]
        [ExpectedException(typeof(CircuitException), "Voltage source loop has been allowed")]
        public void TestVoltageloop2()
        {
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "A", "GND", 1.0),
                new Voltagesource("V2", "A", "B", 1.0),
                new Voltagesource("V3", "B", "C", 1.0),
                new Voltagesource("V4", "C", "D", 1.0),
                new Voltagesource("V5", "D", "E", 1.0),
                new Voltagesource("V6", "E", "F", 1.0),
                new Voltagesource("V7", "F", "G", 1.0),
                new Voltagesource("V8", "G", "H", 1.0),
                new Voltagesource("V9", "H", "I", 1.0),
                new Voltagesource("V10", "I", "0", 1.0)
                );
            CircuitCheck check = new CircuitCheck();
            check.Check(ckt);
        }
    }
}
