using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            }));
            Test<BJTModel>(netlist, "mybjt");
        }
    }
}
