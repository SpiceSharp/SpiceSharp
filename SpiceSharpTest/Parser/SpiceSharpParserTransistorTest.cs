using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Components;

namespace SpiceSharpTest.Parser
{
    [TestClass]
    public class SpiceSharpParserTransistorTest : Framework
    {
        [TestMethod]
        public void MOS1Test()
        {
            var netlist = Run(
                ".model lvl1mod nmos(level = 1 vto = 0.5 kp = 1 gamma = 2 phi = 3 lambda = 4 rd = 5",
                "+ rs = 6 cbd = 7 cbs = 8 is = 9 pb = 10 cgso = 11 cgdo = 12 cgbo = 13 rsh = 14",
                "+ cj = 15 mj = 16 cjsw = 17 mjsw = 18 js = 19 tox = 20 nsub = 21 nss = 22",
                "+ tpg = 24 ld = 26 uo = 27",
                "+ kf = 33 af = 34 fc = 35 tnom = 40)",
                "M1 d g s b lvl1mod l=100u w=200u"
                );
            Test<MOS1Model>(netlist, "lvl1mod", new string[] {
                "vto", "kp", "gamma", "phi", "lambda", "rd",
                "rs", "cbd", "cbs", "is", "pb", "cgso", "cgdo", "cgbo", "rsh",
                "cj", "mj", "cjsw", "mjsw", "js", "tox", "nsub", "nss", 
                "tpg", "ld", "uo", 
                "kf", "af", "fc", "tnom"
            }, new double[] {
                0.5, 1, 2, 3, 4, 5,
                6, 7, 8, 9, 10, 11, 12, 13, 14,
                15, 16, 17, 18, 19, 20, 21, 22, 
                24, 26, 27,
                33, 34, 35, 40
            });
            Test<MOS1>(netlist, "M1", new string[] { "w", "l" }, new double[] { 200e-6, 100e-6 }, new string[] { "d", "g", "s", "b" });
        }

        [TestMethod]
        public void MOS2Test()
        {
            var netlist = Run(".model lvl2mod pmos level = 2 tnom = 0 vto = 1",
                "+ kp = 2 gamma = 3 phi = 4 lambda = 5 rd = 6 rs = 7 cbd = 8",
                "+ cbs = 9 is = 10 pb = 11 cgso = 12 cgdo = 13 cgbo = 14 cj = 15",
                "+ mj = 16 cjsw = 17 mjsw = 18 js = 19 tox = 20 ld = 21 rsh = 22",
                "+ u0 = 23 fc = 24 nsub = 25 tpg = 26 nss = 27 nfs = 28 delta = 29",
                "+ uexp = 30 vmax = 31 xj = 32 neff = 33 ucrit = 34 kf = 35 af = 36",
                "M1 drain gate source bulk lvl2mod w = 5u l = 1u");
            Test<MOS2Model>(netlist, "lvl2mod", new string[]
            {
                "tnom", "vto",
                "kp", "gamma", "phi", "lambda", "rd", "rs", "cbd",
                "cbs", "is", "pb", "cgso", "cgdo", "cgbo", "cj",
                "mj", "cjsw", "mjsw", "js", "tox", "ld", "rsh",
                "u0", "fc", "nsub", "tpg", "nss", "nfs", "delta",
                "uexp", "vmax", "xj", "neff", "ucrit", "kf", "af"
            }, new double[] {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
                16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28,
                29, 30, 31, 32, 33, 34, 35, 36
            });
            Test<MOS2>(netlist, "M1", new string[] { "w", "l" }, new double[] { 5e-6, 1e-6 }, new string[] { "drain", "gate", "source", "bulk" });
        }

        [TestMethod]
        public void MOS3Test()
        {
            var netlist = Run(".model lvl3mod nmos level = 3 vto = 0 kp = 1",
                "+ gamma = 2 phi = 3 rd = 4 rs = 5 cbd = 6 cbs = 7 is = 8",
                "+ pb = 9 cgso = 10 cgdo = 11 cgbo = 12 rsh = 13 cj = 14",
                "+ mj = 15 cjsw = 16 mjsw = 17 js = 18 tox = 19 ld = 20",
                "+ u0 = 21 fc = 22 nsub = 23 tpg = 24 nss = 25 eta = 26",
                "+ nfs = 27 theta = 28 vmax = 29 kappa = 30 xj = 31",
                "+ tnom = 32 kf = 33 af = 34 delta = 38",
                "M1 dr ga so bu lvl3mod w=10u l = 5u");
            Test<MOS3Model>(netlist, "lvl3mod", new string[]
            {
                "vto", "kp",
                "gamma", "phi", "rd", "rs", "cbd", "cbs", "is",
                "pb", "cgso", "cgdo", "cgbo", "rsh", "cj",
                "mj", "cjsw", "mjsw", "js", "tox", "ld",
                "u0", "fc", "nsub", "tpg", "nss", "eta",
                "nfs", "theta", "vmax", "kappa", "xj",
                "tnom", "kf", "af", "input_delta"
            }, new double[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
                16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28,
                29, 30, 31, 32, 33, 34, 38
            });
            Test<MOS3>(netlist, "M1", new string[] { "w", "l" }, new double[] { 10e-6, 5e-6 }, new string[] { "dr", "ga", "so", "bu" });
        }
    }
}
