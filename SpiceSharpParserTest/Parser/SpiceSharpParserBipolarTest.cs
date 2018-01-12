using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharpTest.Parser
{
    [TestClass]
    public class SpiceSharpParserBipolarTest : Framework
    {
        [TestMethod]
        public void BipolarTest()
        {
            var netlist = Run(string.Join(Environment.NewLine + "+ ", new string[] {
                ".model mybjt NPN",
                "is = 1n bf = 50 nf = 1.5 vaf = 400 ikf = 1m",
                "ise = 2p ne = 4p br = 2 nr = 1.8 var = 500 ikr = 2m",
                "isc = 5p nc = 3 rb = 1m irb = 0.8 rbm = 1.1k re = 1.2k",
                "rc = 1.3k cje = 11p vje = 0.99 mje = 0.45 tf = 0.23p",
                "xtf = 1.56 vtf = 100.1 itf = 8.3m ptf = 10 cjc = 543p",
                "vjc = 1.92 mjc = 0.4567 xcjc = 0.82 tr = 13n cjs = 2.22p",
                "vjs = 1.93 mjs = 0.4568 xtb = 0.85 eg = 1.143 xti = 4 kf = 0.1",
                "af = 1.111 fc = 0.898 tnom = 30.123"
            }), "Q1 a b c d mybjt");
            Test<BJTModel>(netlist, new Identifier("mybjt"), new string[] { "is", "bf", "nf", "vaf", "ikf",
                "ise", "ne", "br", "nr", "var", "ikr",
                "isc", "nc", "rb", "irb", "rbm", "re",
                "rc", "cje", "vje", "mje", "tf",
                "xtf", "vtf", "itf", "ptf", "cjc",
                "vjc", "mjc", "xcjc", "tr", "cjs",
                "vjs", "mjs", "xtb", "eg", "xti", "kf",
                "af", "fc", "tnom"
            }, new double[] {
                1e-9, 50.0, 1.5, 400.0, 1e-3,
                2e-12, 4e-12, 2.0, 1.8, 500.0, 2e-3,
                5e-12, 3, 1e-3, 0.8, 1.1e3, 1.2e3,
                1.3e3, 11e-12, 0.99, 0.45, 0.23e-12,
                1.56, 100.1, 8.3e-3, 10.0, 543e-12,
                1.92, 0.4567, 0.82, 13e-9, 2.22e-12,
                1.93, 0.4568, 0.85, 1.143, 4, 0.1,
                1.111, 0.898, 30.123
            });
            Test<BJT>(netlist, new Identifier("Q1"), null, null, new Identifier[] {
                new Identifier("a"),
                new Identifier("b"),
                new Identifier("c"),
                new Identifier("d")
            });
        }
    }
}
