using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharpTest.Parser
{
    [TestClass]
    public class SpiceSharpParserSourcesTest : Framework
    {
        [TestMethod]
        public void VoltagesourceTest()
        {
            var netlist = Run("Vinput in g-n-d 5.0",
                "Vsupply_2 a b AC 2 3",
                "v2 b c PULSE(0 1 2 3 4 5 6)",
                "v3 b c SINE(1 2 3 4 5)");

            Test<Voltagesource>(netlist, new CircuitIdentifier("Vinput"), new string[] { "dc" }, new double[] { 5.0 }, new CircuitIdentifier[] {
                new CircuitIdentifier("in"),
                new CircuitIdentifier("g-n-d")
            });
            Test<Voltagesource>(netlist, new CircuitIdentifier("Vsupply_2"), new string[] { "acmag", "acphase" }, new double[] { 2.0, 3.0 }, new CircuitIdentifier[] {
                new CircuitIdentifier("a"),
                new CircuitIdentifier("b")
            });
            var vsrc = Test<Voltagesource>(netlist, new CircuitIdentifier("v2"));
            TestParameters(vsrc.VSRCwaveform, new string[] { "v1", "v2", "td", "tr", "tf", "pw", "per" }, new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 });
            vsrc = Test<Voltagesource>(netlist, new CircuitIdentifier("v3"));
            TestParameters(vsrc.VSRCwaveform, new string[] { "vo", "va", "freq", "td", "theta" }, new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 });
        }

        [TestMethod]
        public void CurrentsourceTest()
        {
            var netlist = Run("Iinput in g-n-d 5.0",
                "Isupply_2 a b AC 2 3",
                "i2 b c PULSE(0 1 2 3 4 5 6)",
                "i3 b c SINE(1 2 3 4 5)");

            Test<Currentsource>(netlist, new CircuitIdentifier("Iinput"), new string[] { "dc" }, new double[] { 5.0 }, new CircuitIdentifier[] {
                new CircuitIdentifier("in"),
                new CircuitIdentifier("g-n-d")
            });
            Test<Currentsource>(netlist, new CircuitIdentifier("Isupply_2"), new string[] { "acmag", "acphase" }, new double[] { 2.0, 3.0 }, new CircuitIdentifier[] {
                new CircuitIdentifier("a"),
                new CircuitIdentifier("b")
            });
            var isrc = Test<Currentsource>(netlist, new CircuitIdentifier("i2"));
            TestParameters(isrc.ISRCwaveform, new string[] { "v1", "v2", "td", "tr", "tf", "pw", "per" }, new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 });
            isrc = Test<Currentsource>(netlist, new CircuitIdentifier("i3"));
            TestParameters(isrc.ISRCwaveform, new string[] { "vo", "va", "freq", "td", "theta" }, new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 });
        }

        [TestMethod]
        public void ControlledSourceTest()
        {
            var netlist = Run("G1 2 0 5 0 0.1mmho",
                "e1 2 3 14 1 2.0",
                "f1 13 5 VSENS 5",
                "HX 5 17 VZ 0.5k");
            Test<VoltageControlledCurrentsource>(netlist, new CircuitIdentifier("G1"), new string[] { "gain" }, new double[] { 0.1e-3 }, new CircuitIdentifier[] {
                new CircuitIdentifier("2"),
                new CircuitIdentifier("0"),
                new CircuitIdentifier("5"),
                new CircuitIdentifier("0")
            });
            Test<VoltageControlledVoltagesource>(netlist, new CircuitIdentifier("e1"), new string[] { "gain" }, new double[] { 2.0 }, new CircuitIdentifier[] {
                new CircuitIdentifier("2"),
                new CircuitIdentifier("3"),
                new CircuitIdentifier("14"),
                new CircuitIdentifier("1")
            });
            var f = Test<CurrentControlledCurrentsource>(netlist, new CircuitIdentifier("f1"), new string[] { "gain" }, new double[] { 5.0 }, new CircuitIdentifier[] {
                new CircuitIdentifier("13"),
                new CircuitIdentifier("5")
            });
            Assert.AreEqual(f.CCCScontName, new CircuitIdentifier("VSENS"));
            var h = Test<CurrentControlledVoltagesource>(netlist, new CircuitIdentifier("HX"), new string[] { "gain" }, new double[] { 0.5e3 }, new CircuitIdentifier[] {
                new CircuitIdentifier("5"),
                new CircuitIdentifier("17")
            });
        }
    }
}
