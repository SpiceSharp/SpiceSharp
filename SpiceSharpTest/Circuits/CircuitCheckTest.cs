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
            // Verifies that CircuitException is thrown during Check when circuit has a ground node called "GND"
            var ckt = CreateCircuit("GND");
            ckt.Check();
        }

        [TestMethod]
        public void CorrectGroundNodeName()
        {
            // Verifies that CircuitException is not thrown during Check when circuit has a ground node called "gnd"
            var ckt = CreateCircuit("gnd");
            ckt.Check();
        }

        [TestMethod]
        public void CorrectSecondGroundNodeName()
        {
            // Verifies that CircuitException is not thrown during Check when circuit has a ground node called "0"
            var ckt = CreateCircuit("0");
            ckt.Check();
        }

        /// <summary>
        /// Creates a circuit with a resistor and a voltage source which is connected to IN
        /// node and a ground node with name <paramref name="groundNodeName"/>
        /// </summary>
        /// <param name="groundNodeName"></param>
        /// <returns></returns>
        private static Circuit CreateCircuit(string groundNodeName)
        {
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource(
                    new Identifier("V_1"),
                    new Identifier("IN"),
                    new Identifier(groundNodeName),
                    1),
                new Resistor(
                    new Identifier("R_1"),
                    new Identifier("IN"),
                    new Identifier(groundNodeName),
                    1)
            );
            return ckt;
        }
    }
}
