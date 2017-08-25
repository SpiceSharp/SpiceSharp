using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Components;

namespace SpiceSharpTest.Parser
{
    [TestClass]
    public class SpiceSharpParserRLCMTest : Framework
    {

        [TestMethod]
        public void ResistorTest()
        {
            // Normal resistor
            var netlist = Run("R_1 in OUT 10k");
            Test<Resistor>(netlist, "R_1", new string[] { "resistance" }, new double[] { 10e3 }, new string[] { "in", "out" });

            // Semiconductor resistor
            netlist = Run(".MODEL mymodel R(tc1 = 1.0 tc2 = 2.0 rsh = 1k defw = 10u narrow = 0.1u tnom = 30)",
                "Rmod 1 0 mymodel l = 1u w = 10u",
                "Rmod2 4 1 mymodel l = 10u");
            Initialize(netlist);
            Test<ResistorModel>(netlist, "mymodel", new string[] { "tc1", "tc2", "rsh", "defw", "narrow", "tnom" }, new double[] { 1.0, 2.0, 1e3, 10e-6, 0.1e-6, 30.0 });
            Test<Resistor>(netlist, "Rmod", new string[] { "l", "w" }, new double[] { 1e-6, 10e-6 }, new string[] { "1", "0" });
            Test<Resistor>(netlist, "Rmod2", new string[] { "l", "w" }, new double[] { 10e-6, 10e-6 }, new string[] { "4", "1" });
        }

        [TestMethod]
        public void CapacitorTest()
        {
            // Normal capacitor
            var netlist = Run("C_L OUT GND 1p ic=10");
            Test<Capacitor>(netlist, "C_L", new string[] { "capacitance", "ic" }, new double[] { 1e-12, 10.0 }, new string[] { "OUT", "GND" });

            // Semiconductor capacitor
            netlist = Run(".MODEL c_mod C cj = 10u cjsw = 20u defw = 30u narrow = 0.5n",
                "Cmod1 a b c_mod w = 100u l = 200u",
                "Cmod2 asdf lkjh c_mod l = 4u ic=5");
            Initialize(netlist);
            Test<CapacitorModel>(netlist, "c_mod", new string[] { "cj", "cjsw", "defw", "narrow" }, new double[] { 10e-6, 20e-6, 30e-6, 0.5e-9 });
            Test<Capacitor>(netlist, "Cmod1", new string[] { "w", "l" }, new double[] { 100e-6, 200e-6 }, new string[] { "a", "b" });
            Test<Capacitor>(netlist, "Cmod2", new string[] { "w", "l", "ic" }, new double[] { 30e-6, 4e-6, 5.0 }, new string[] { "asdf", "lkjh" });
        }

        [TestMethod]
        public void InductorTest()
        {
            var netlist = Run("lxyz np nm 1m IC=10");
            Test<Inductor>(netlist, "lxyz", new string[] { "inductance", "ic" }, new double[] { 1e-3, 10.0 }, new string[] { "np", "nm" });
        }

        [TestMethod]
        public void CoupledInductorTest()
        {
            var netlist = Run("L1 A GND 1m",
                "L2 B GND 2m",
                "K1 L1 L2 0.5");
            var m = (MutualInductance)Test<MutualInductance>(netlist, "k1", new string[] { "k" }, new double[] { 0.5 });
            Assert.AreEqual(m.MUTind1, "l1");
            Assert.AreEqual(m.MUTind2, "l2");
        }
    }
}
