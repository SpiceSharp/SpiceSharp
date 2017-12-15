using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharpTest.Parser
{
    [TestClass]
    public class SpiceSharpParserDiodeTest : Framework
    {
        [TestMethod]
        public void DiodeTest()
        {
            var netlist = Run(".model diomod d(is = 1n rs = 5 n = 1.5 tt = 1p cjo = 2p vj = 0.75",
                "+ m = 0.6 eg = 1.14 xti = 2.1 kf = 0.1 af = 1.5 fc = 0.8 bv = 5 ibv = 10m tnom = 30)",
                "D1 IN OUT diomod");
            Test<DiodeModel>(netlist, new Identifier("diomod"), new string[] { "is", "rs", "n", "tt", "cjo", "vj", "m", "eg", "xti", "kf", "af", "fc", "bv", "ibv", "tnom" },
                new double[] { 1e-9, 5.0, 1.5, 1e-12, 2e-12, 0.75, 0.6, 1.14, 2.1, 0.1, 1.5, 0.8, 5.0, 10e-3, 30.0 });
            Test<Diode>(netlist, new Identifier("D1"), null, null, new Identifier[] {
                new Identifier("IN"),
                new Identifier("OUT")
            });
        }
    }
}
