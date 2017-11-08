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
                new Voltagesource(new CircuitIdentifier("V1"), new CircuitIdentifier("A"), new CircuitIdentifier("0"), 1.0),
                new Voltagesource(new CircuitIdentifier("V2"), new CircuitIdentifier("B"), new CircuitIdentifier("A"), 1.0),
                new Voltagesource(new CircuitIdentifier("V3"), new CircuitIdentifier("B"), new CircuitIdentifier("A"), 1.0)
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
                new Voltagesource(new CircuitIdentifier("V1"), new CircuitIdentifier("A"), new CircuitIdentifier("0"), 1.0),
                new Voltagesource(new CircuitIdentifier("V2"), new CircuitIdentifier("A"), new CircuitIdentifier("B"), 1.0),
                new Voltagesource(new CircuitIdentifier("V3"), new CircuitIdentifier("B"), new CircuitIdentifier("C"), 1.0),
                new Voltagesource(new CircuitIdentifier("V4"), new CircuitIdentifier("C"), new CircuitIdentifier("D"), 1.0),
                new Voltagesource(new CircuitIdentifier("V5"), new CircuitIdentifier("D"), new CircuitIdentifier("E"), 1.0),
                new Voltagesource(new CircuitIdentifier("V6"), new CircuitIdentifier("E"), new CircuitIdentifier("F"), 1.0),
                new Voltagesource(new CircuitIdentifier("V7"), new CircuitIdentifier("F"), new CircuitIdentifier("G"), 1.0),
                new Voltagesource(new CircuitIdentifier("V8"), new CircuitIdentifier("G"), new CircuitIdentifier("H"), 1.0),
                new Voltagesource(new CircuitIdentifier("V9"), new CircuitIdentifier("H"), new CircuitIdentifier("I"), 1.0),
                new Voltagesource(new CircuitIdentifier("V10"), new CircuitIdentifier("I"), new CircuitIdentifier("0"), 1.0)
                );
            CircuitCheck check = new CircuitCheck();
            check.Check(ckt);
        }

        [TestMethod]
        [ExpectedException(typeof(CircuitException), "Floating node not detected")]
        public void TestFloatingNode1()
        {
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource(new CircuitIdentifier("V1"), new CircuitIdentifier("in"), new CircuitIdentifier("gnd"), 1.0),
                new Capacitor(new CircuitIdentifier("C1"), new CircuitIdentifier("in"), new CircuitIdentifier("out"), 1e-12),
                new Capacitor(new CircuitIdentifier("C2"), new CircuitIdentifier("out"), new CircuitIdentifier("gnd"), 1e-12)
                );
            CircuitCheck check = new CircuitCheck();
            check.Check(ckt);
        }

        [TestMethod]
        [ExpectedException(typeof(CircuitException), "Floating node not detected")]
        public void TestFloatingNode2()
        {
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource(new CircuitIdentifier("V1"), new CircuitIdentifier("input"), new CircuitIdentifier("gnd"), 1.0),
                new VoltageControlledVoltagesource(new CircuitIdentifier("E1"), new CircuitIdentifier("out"), new CircuitIdentifier("gnd"), new CircuitIdentifier("in"), new CircuitIdentifier("gnd"), 2.0),
                new VoltageControlledVoltagesource(new CircuitIdentifier("E2"), new CircuitIdentifier("out2"), new CircuitIdentifier("gnd"), new CircuitIdentifier("out"), new CircuitIdentifier("gnd"), 1.0)
                );
            CircuitCheck check = new CircuitCheck();
            check.Check(ckt);
        }
    }
}
