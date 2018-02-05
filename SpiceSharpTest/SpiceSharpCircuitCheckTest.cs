using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;

namespace SpiceSharpTest.Validation
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
                new VoltageSource(new Identifier("V1"), new Identifier("A"), new Identifier("0"), 1.0),
                new VoltageSource(new Identifier("V2"), new Identifier("B"), new Identifier("A"), 1.0),
                new VoltageSource(new Identifier("V3"), new Identifier("B"), new Identifier("A"), 1.0)
                );

            Validator check = new Validator();
            check.Validate(ckt);
        }

        [TestMethod]
        [ExpectedException(typeof(CircuitException), "Voltage source loop has been allowed")]
        public void TestVoltageloop2()
        {
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource(new Identifier("V1"), new Identifier("A"), new Identifier("0"), 1.0),
                new VoltageSource(new Identifier("V2"), new Identifier("A"), new Identifier("B"), 1.0),
                new VoltageSource(new Identifier("V3"), new Identifier("B"), new Identifier("C"), 1.0),
                new VoltageSource(new Identifier("V4"), new Identifier("C"), new Identifier("D"), 1.0),
                new VoltageSource(new Identifier("V5"), new Identifier("D"), new Identifier("E"), 1.0),
                new VoltageSource(new Identifier("V6"), new Identifier("E"), new Identifier("F"), 1.0),
                new VoltageSource(new Identifier("V7"), new Identifier("F"), new Identifier("G"), 1.0),
                new VoltageSource(new Identifier("V8"), new Identifier("G"), new Identifier("H"), 1.0),
                new VoltageSource(new Identifier("V9"), new Identifier("H"), new Identifier("I"), 1.0),
                new VoltageSource(new Identifier("V10"), new Identifier("I"), new Identifier("0"), 1.0)
                );
            Validator check = new Validator();
            check.Validate(ckt);
        }

        [TestMethod]
        [ExpectedException(typeof(CircuitException), "Floating node not detected")]
        public void TestFloatingNode1()
        {
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource(new Identifier("V1"), new Identifier("in"), new Identifier("gnd"), 1.0),
                new Capacitor(new Identifier("C1"), new Identifier("in"), new Identifier("out"), 1e-12),
                new Capacitor(new Identifier("C2"), new Identifier("out"), new Identifier("gnd"), 1e-12)
                );
            Validator check = new Validator();
            check.Validate(ckt);
        }

        [TestMethod]
        [ExpectedException(typeof(CircuitException), "Floating node not detected")]
        public void TestFloatingNode2()
        {
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource(new Identifier("V1"), new Identifier("input"), new Identifier("gnd"), 1.0),
                new VoltageControlledVoltageSource(new Identifier("E1"), new Identifier("out"), new Identifier("gnd"), new Identifier("in"), new Identifier("gnd"), 2.0),
                new VoltageControlledVoltageSource(new Identifier("E2"), new Identifier("out2"), new Identifier("gnd"), new Identifier("out"), new Identifier("gnd"), 1.0)
                );
            Validator check = new Validator();
            check.Validate(ckt);
        }
    }
}
