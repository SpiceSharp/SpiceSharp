using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Components;
using SpiceSharp.Parser;

namespace SpiceSharpTest.Parser
{
    [TestClass]
    public class SpiceSharpParserSubcircuitTest : Framework
    {
        /**
         * Please note that subcircuits in this framework behave a bit different:
         * Subcircuits are not flattened like in Spice. Subcircuits in SpiceSharp behave
         * like a container that contain multiple components. The nodes are resolved when
         * setting up the circuit. This makes them contained by themselves, so you can 
         * move them around as you see fit.
         * Parameters are not supported on the circuit-level, but they are on the parser-
         * level.
         **/

        [TestMethod]
        public void SubcircuitSimpleTest()
        {
            var netlist = Run(
                ".SUBCKT custom A B R=10k C=1u",
                "Rseries A B {R}",
                "Cload B GND {C}",
                ".ENDS",
                "X1 IN OUT custom"
                );
            Test<Resistor>(netlist, new string[] { "X1", "Rseries" }, new string[] { "resistance" }, new double[] { 10e3 }, new string[] { "A", "B" });
            Test<Capacitor>(netlist, new string[] { "X1", "Cload" }, new string[] { "capacitance" }, new double[] { 1e-6 }, new string[] { "B", "GND" });
            Test<Subcircuit>(netlist, "X1", null, null, new string[] { "IN", "OUT" });
        }

        [TestMethod]
        public void SubcircuitNestedTest()
        {
            var netlist = Run(
                "x1 in out lpfilter",

                ".subckt lpfilter A B",

                ".subckt single A B PARAMS: R=1",
                "r1 A B {R*1k}",
                "c1 B GND 1u",
                ".ends",
                
                "xfirst A B1 single R=2",
                "xsecond B1 B2 single R=5",
                "xthird B2 B single R=10",
                ".ends"
                );

            // Test parameters
            Test<Resistor>(netlist, new string[] { "x1", "xfirst", "r1" }, new string[] { "resistance" }, new double[] { 2e3 }, new string[] { "A", "B" });
            Test<Resistor>(netlist, new string[] { "x1", "xsecond", "r1" }, new string[] { "resistance" }, new double[] { 5e3 }, new string[] { "A", "B" });
            Test<Resistor>(netlist, new string[] { "x1", "xthird", "r1" }, new string[] { "resistance" }, new double[] { 10e3 }, new string[] { "A", "B" });
            Test<Capacitor>(netlist, new string[] { "x1", "xfirst", "c1" }, new string[] { "capacitance" }, new double[] { 1e-6 }, new string[] { "B", "GND" });
            Test<Capacitor>(netlist, new string[] { "x1", "xsecond", "c1" }, new string[] { "capacitance" }, new double[] { 1e-6 }, new string[] { "B", "GND" });
            Test<Capacitor>(netlist, new string[] { "x1", "xthird", "c1" }, new string[] { "capacitance" }, new double[] { 1e-6 }, new string[] { "B", "GND" });

            // Test connections
            Test<Subcircuit>(netlist, new string[] { "x1", "xfirst" }, null, null, new string[] { "A", "B1" });
            Test<Subcircuit>(netlist, new string[] { "x1", "xsecond" }, null, null, new string[] { "B1", "B2" });
            Test<Subcircuit>(netlist, new string[] { "x1", "xthird" }, null, null, new string[] { "B2", "B" });
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException), "Recursive loops in subcircuits have been allowed")]
        public void RecursiveSubcircuitDefinition()
        {
            var netlist = Run(
                "x1 in out lpfilter",

                ".subckt lpfilter A B",

                ".subckt single A B PARAMS: R=1",
                "r1 A B {R*1k}",
                "x1 A B lpfilter",
                ".ends",

                "xfirst A B1 single R=2",
                "xsecond B1 B2 single R=5",
                "xthird B2 B single R=10",
                ".ends"
                );
        }
    }
}
