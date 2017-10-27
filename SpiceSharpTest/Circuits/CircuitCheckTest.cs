using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;

namespace SpiceSharpTest.Circuits
{
    [TestClass]
    public class CircuitCheckTest
    {
        [TestMethod]
        [ExpectedException(typeof(CircuitException))]
        public void BadGroundNodeName()
        {
            var ckt = CreateCircuit("GND");
            ckt.Check();
        }

        [TestMethod]
        public void CorrectGroundNodeName()
        {
            var ckt = CreateCircuit("gnd");
            ckt.Check();
        }

        [TestMethod]
        public void CorrectSecondGroundNodeName()
        {
            var ckt = CreateCircuit("0");
            ckt.Check();
        }

        private static Circuit CreateCircuit(string groundNodeName)
        {
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource(
                    new CircuitIdentifier("V_1"),
                    new CircuitIdentifier("IN"),
                    new CircuitIdentifier(groundNodeName),
                    1),
                new SpiceSharp.Components.Resistor(
                    new CircuitIdentifier("R_1"),
                    new CircuitIdentifier("IN"),
                    new CircuitIdentifier(groundNodeName),
                    1)
            );
            return ckt;
        }
    }
}
