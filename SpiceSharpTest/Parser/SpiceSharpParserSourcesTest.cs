using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Components;

namespace SpiceSharpTest.Parser
{
    [TestClass]
    public class SpiceSharpParserSourcesTest : Framework
    {
        [TestMethod]
        public void VoltagesourceTest()
        {
            var netlist = Run("Vinput in g-n-d 5.0" + Environment.NewLine +
                "Vsupply_2 a b AC 2 3" + Environment.NewLine +
                "v2 b c PULSE(0 1 2 3 4 5 6)" + Environment.NewLine +
                "v3 b c SINE(1 2 3 4 5)");

            Test<Voltagesource>(netlist, "Vinput", new string[] { "dc" }, new double[] { 5.0 }, new string[] { "in", "g-n-d" });
            Test<Voltagesource>(netlist, "Vsupply_2", new string[] { "acmag", "acphase" }, new double[] { 2.0, 3.0 }, new string[] { "a", "b" });
            var vsrc = Test<Voltagesource>(netlist, "v2");
            TestParameters(vsrc.VSRCwaveform, new string[] { "v1", "v2", "td", "tr", "tf", "pw", "per" }, new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 });
            vsrc = Test<Voltagesource>(netlist, "v3");
            TestParameters(vsrc.VSRCwaveform, new string[] { "vo", "va", "freq", "td", "theta" }, new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 });
        }

        [TestMethod]
        public void CurrentsourceTest()
        {
            var netlist = Run("Iinput in g-n-d 5.0" + Environment.NewLine +
                "Isupply_2 a b AC 2 3" + Environment.NewLine +
                "i2 b c PULSE(0 1 2 3 4 5 6)" + Environment.NewLine +
                "i3 b c SINE(1 2 3 4 5)");

            Test<Currentsource>(netlist, "Iinput", new string[] { "dc" }, new double[] { 5.0 }, new string[] { "in", "g-n-d" });
            Test<Currentsource>(netlist, "Isupply_2", new string[] { "acmag", "acphase" }, new double[] { 2.0, 3.0 }, new string[] { "a", "b" });
            var isrc = Test<Currentsource>(netlist, "i2");
            TestParameters(isrc.ISRCwaveform, new string[] { "v1", "v2", "td", "tr", "tf", "pw", "per" }, new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 });
            isrc = Test<Currentsource>(netlist, "i3");
            TestParameters(isrc.ISRCwaveform, new string[] { "vo", "va", "freq", "td", "theta" }, new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 });
        }

        [TestMethod]
        public void ControlledSourceTest()
        {
            var netlist = Run("G1 2 0 5 0 0.1mmho" + Environment.NewLine +
                "e1 2 3 14 1 2.0" + Environment.NewLine +
                "f1 13 5 VSENS 5" + Environment.NewLine + 
                "HX 5 17 VZ 0.5k");
            Test<VoltageControlledCurrentsource>(netlist, "g1", new string[] { "gain" }, new double[] { 0.1e-3 }, new string[] { "2", "0", "5", "0" });
            Test<VoltageControlledVoltagesource>(netlist, "e1", new string[] { "gain" }, new double[] { 2.0 }, new string[] { "2", "3", "14", "1" });
            var f = Test<CurrentControlledCurrentsource>(netlist, "f1", new string[] { "gain" }, new double[] { 5.0 }, new string[] { "13", "5" });
            Assert.AreEqual(f.CCCScontName, "vsens");
            var h = Test<CurrentControlledVoltagesource>(netlist, "HX", new string[] { "gain" }, new double[] { 0.5e3 }, new string[] { "5", "17" });
        }
    }
}
