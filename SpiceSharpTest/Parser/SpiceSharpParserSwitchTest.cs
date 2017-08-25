using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Components;

namespace SpiceSharpTest.Parser
{
    [TestClass]
    public class SpiceSharpParserSwitchTest : Framework
    {
        [TestMethod]
        public void SwitchTest()
        {
            var netlist = Run(".model mod_sw SW(vt = 1 vh = 3 ron = 1e-9 roff = 1k)",
                ".model mod_csw csw it = 2 ih = 0.5 ron = 10k roff = 1k",
                "switch1 1 2 10 0 mod_sw",
                "wreset 5 6 vclck mod_csw OFF");
            Test<VoltageSwitchModel>(netlist, "mod_sw", new string[] { "vt", "vh", "ron", "roff" }, new double[] { 1.0, 3.0, 1e-9, 1e3 });
            Test<CurrentSwitchModel>(netlist, "mod_csw", new string[] { "it", "ih", "ron", "roff" }, new double[] { 2.0, 0.5, 10e3, 1e3 });
            Test<VoltageSwitch>(netlist, "switch1", null, null, new string[] { "1", "2", "10", "0" });
            var w = Test<CurrentSwitch>(netlist, "wreset", null, null, new string[] { "5", "6" });

            // The ON/OFF cannot be tested :-(
        }
    }
}
