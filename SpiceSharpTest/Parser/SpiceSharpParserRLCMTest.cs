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
            var netlist = Run("R_1 in OUT 10k");

            // Check the netlist
            var r = (Resistor)netlist.Circuit.Objects["r_1"];
            Assert.AreEqual(r.RESresist, 10e3, 1e-12);
            Assert.AreEqual(r.GetNode(0), "in");
            Assert.AreEqual(r.GetNode(1), "out");

            // Add a model
            netlist = Run(".MODEL mymodel R(tc1 = 1.0 tc2 = 2.0 rsh = 1k defw = 10u narrow = 0.1u tnom = 30)" + Environment.NewLine +
                "Rmod 1 0 mymodel l = 1u w = 10u" + Environment.NewLine +
                "Rmod2 4 1 mymodel l = 10u");
            var rm = (ResistorModel)netlist.Circuit.Objects["mymodel"];
            Assert.AreEqual(rm.REStempCoeff1, 1.0, 1e-12);
            Assert.AreEqual(rm.REStempCoeff2, 2.0, 1e-12);
            Assert.AreEqual(rm.RESsheetRes, 1e3, 1e-12);
            Assert.AreEqual(rm.RESdefWidth, 10e-6, 1e-12);
            Assert.AreEqual(rm.RESnarrow, 0.1e-6, 1e-12);
            Assert.AreEqual(rm.RES_TNOM, 30.0, 1e-12);

            r = (Resistor)netlist.Circuit.Objects["rmod"];
            Assert.AreEqual(r.Model, rm);
            Assert.AreEqual(r.RESlength, 1e-6, 1e-12);
            Assert.AreEqual(r.RESwidth, 10e-6, 1e-12);
            r = (Resistor)netlist.Circuit.Objects["rmod2"];
            Assert.AreEqual(r.Model, rm);
            Assert.AreEqual(r.RESlength, 10e-6, 1e-12);

            // Force a setup and temp to check for the width
            r.Setup(netlist.Circuit);
            r.Temperature(netlist.Circuit);
            Assert.AreEqual(r.RESwidth, 10e-6, 1e-12);
        }

        [TestMethod]
        public void CapacitorTest()
        {
            var n = Run("C_L OUT GND 1p");

            var c = (Capacitor)n.Circuit.Objects["c_l"];
            Assert.AreEqual(c.CAPcapac, 1e-12, 1e-15);
            Assert.AreEqual(c.GetNode(0), "out");
            Assert.AreEqual(c.GetNode(1), "gnd");

            n = Run(".MODEL c_mod C cj = 10u cjsw = 20u defw = 30u narrow = 0.5n" + Environment.NewLine +
                "Cmod1 a b c_mod w = 100u l = 200u" + Environment.NewLine +
                "Cmod2 asdf lkjh c_mod l = 4u ic=5");

            var cm = (CapacitorModel)n.Circuit.Objects["c_mod"];
            Assert.AreEqual(cm.CAPcj, 10e-6, 1e-12);
            Assert.AreEqual(cm.CAPcjsw, 20e-6, 1e-12);
            Assert.AreEqual(cm.CAPdefWidth, 30e-6, 1e-12);
            Assert.AreEqual(cm.CAPnarrow, 0.5e-9, 1e-12);

            c = (Capacitor)n.Circuit.Objects["cmod1"];
            Assert.AreEqual(c.Model, cm);
            Assert.AreEqual(c.CAPwidth, 100e-6, 1e-12);
            Assert.AreEqual(c.CAPlength, 200e-6, 1e-12);

            c = (Capacitor)n.Circuit.Objects["cmod2"];
            Assert.AreEqual(c.Model, cm);
            Assert.AreEqual(c.CAPlength, 4e-6, 1e-12);
            Assert.AreEqual(c.CAPinitCond, 5.0, 1e-12);
            c.Setup(n.Circuit);
            c.Temperature(n.Circuit);
            Assert.AreEqual(c.CAPwidth, 30e-6, 1e-12);
        }

        [TestMethod]
        public void InductorTest()
        {
            var n = Run("lxyz np nm 1m IC=10");

            var l = (Inductor)n.Circuit.Objects["lxyz"];
            Assert.AreEqual(l.GetNode(0), "np");
            Assert.AreEqual(l.GetNode(1), "nm");
            Assert.AreEqual(l.INDinduct, 1e-3, 1e-12);
            Assert.AreEqual(l.INDinitCond, 10.0, 1e-12);
        }

        [TestMethod]
        public void CoupledInductorTest()
        {
            var n = Run("L1 A GND 1m" + Environment.NewLine +
                "L2 B GND 2m" + Environment.NewLine +
                "K1 L1 L2 0.5");

            var m = (MutualInductance)n.Circuit.Objects["k1"];
            Assert.AreEqual(m.MUTind1, "l1");
            Assert.AreEqual(m.MUTind2, "l2");
            Assert.AreEqual(m.MUTcoupling, 0.5, 1e-12);
        }
    }
}
