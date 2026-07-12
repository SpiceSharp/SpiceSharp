using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class ExportPropertyRegressionTests
    {
        private static void AssertComplex(Complex actual, Complex expected)
        {
            Assert.That(actual.Real, Is.EqualTo(expected.Real).Within(1e-12));
            Assert.That(actual.Imaginary, Is.EqualTo(expected.Imaginary).Within(1e-12));
        }

        [Test]
        public void When_ReferenceOnlyVoltageIsExported_Expect_NegatedValue()
        {
            var circuit = new Circuit(
                new VoltageSource("V1", "in", "0", 2.0).SetParameter("ac", new[] { 2.0, 90.0 }),
                new Resistor("R1", "in", "0", 1.0));

            var op = new OP("op");
            var real = new RealVoltageExport(op, default, "in");
            foreach (var _ in op.Run(circuit, OP.ExportOperatingPoint))
                Assert.That(real.Value, Is.EqualTo(-2.0).Within(1e-12));

            var ac = new AC("ac", new LinearSweep(1.0, 1.0, 1));
            var complex = new ComplexVoltageExport(ac, default, "in");
            foreach (var _ in ac.Run(circuit, AC.ExportSmallSignal))
                AssertComplex(complex.Value, new Complex(0.0, -2.0));
        }

        [Test]
        public void When_CurrentSourceHasMultiplier_Expect_ScaledAcProperties()
        {
            var source = new CurrentSource("I1", "0", "out", 0.0)
                .SetParameter("acmag", 1.0)
                .SetParameter("m", 2.0);
            var circuit = new Circuit(source, new Resistor("R1", "out", "0", 1.0));
            var ac = new AC("ac", new LinearSweep(1.0, 1.0, 1));
            var voltage = new ComplexPropertyExport(ac, "I1", "v");
            var current = new ComplexPropertyExport(ac, "I1", "i");
            var power = new ComplexPropertyExport(ac, "I1", "p");

            foreach (var _ in ac.Run(circuit, AC.ExportSmallSignal))
            {
                AssertComplex(current.Value, new Complex(2.0, 0.0));
                AssertComplex(power.Value, -voltage.Value * Complex.Conjugate(current.Value));
            }
        }

        [Test]
        public void When_ControlledSourcesHaveMultiplier_Expect_ScaledProperties()
        {
            var vccs = new VoltageControlledCurrentSource("G1", "0", "gout", "in", "0", 1.0)
                .SetParameter("m", 2.0);
            var cccs = new CurrentControlledCurrentSource("F1", "0", "fout", "V1", 1.0)
                .SetParameter("m", 2.0);
            var circuit = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0).SetParameter("acmag", 1.0),
                new Resistor("Rin", "in", "0", 1.0),
                vccs,
                new Resistor("Rg", "gout", "0", 1.0),
                cccs,
                new Resistor("Rf", "fout", "0", 1.0));

            var op = new OP("op");
            var vccsCurrent = new RealPropertyExport(op, "G1", "i");
            var cccsCurrent = new RealPropertyExport(op, "F1", "i");
            foreach (var _ in op.Run(circuit, OP.ExportOperatingPoint))
            {
                Assert.That(Math.Abs(vccsCurrent.Value), Is.EqualTo(2.0).Within(1e-12));
                Assert.That(Math.Abs(cccsCurrent.Value), Is.EqualTo(2.0).Within(1e-12));
            }

            var ac = new AC("ac", new LinearSweep(1.0, 1.0, 1));
            var complexVccsCurrent = new ComplexPropertyExport(ac, "G1", "i");
            var complexCccsCurrent = new ComplexPropertyExport(ac, "F1", "i");
            foreach (var _ in ac.Run(circuit, AC.ExportSmallSignal))
            {
                Assert.That(complexVccsCurrent.Value.Magnitude, Is.EqualTo(2.0).Within(1e-12));
                Assert.That(complexCccsCurrent.Value.Magnitude, Is.EqualTo(2.0).Within(1e-12));
            }
        }

        [Test]
        public void When_ReactiveAndIndependentComponentsExportPower_Expect_ComplexPowerIdentity()
        {
            var circuit = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0).SetParameter("ac", new[] { 1.0, 90.0 }),
                new Inductor("L1", "in", "out", 1.0 / (2.0 * Math.PI)),
                new Resistor("R1", "out", "0", 1.0));
            var ac = new AC("ac", new LinearSweep(1.0, 1.0, 1));
            var sourceVoltage = new ComplexPropertyExport(ac, "V1", "v");
            var sourceCurrent = new ComplexPropertyExport(ac, "V1", "i");
            var sourcePower = new ComplexPropertyExport(ac, "V1", "p");
            var inductorVoltage = new ComplexPropertyExport(ac, "L1", "v");
            var inductorCurrent = new ComplexPropertyExport(ac, "L1", "i");
            var inductorPower = new ComplexPropertyExport(ac, "L1", "p");

            foreach (var _ in ac.Run(circuit, AC.ExportSmallSignal))
            {
                AssertComplex(sourcePower.Value, -sourceVoltage.Value * Complex.Conjugate(sourceCurrent.Value));
                AssertComplex(inductorPower.Value, -inductorVoltage.Value * Complex.Conjugate(inductorCurrent.Value));
            }
        }

        [Test]
        public void When_SwitchExportsAcProperties_Expect_ScaledCurrentAndPower()
        {
            var circuit = new Circuit(
                new VoltageSource("Vctrl", "ctrl", "0", 1.0),
                new VoltageSource("Vac", "supply", "0", 0.0).SetParameter("acmag", 1.0),
                new Resistor("R1", "supply", "out", 1.0),
                new VoltageSwitch("S1", "out", "0", "ctrl", "0", "SM").SetParameter("m", 2.0),
                new VoltageSwitchModel("SM")
                    .SetParameter("vt", 0.5)
                    .SetParameter("ron", 1.0)
                    .SetParameter("roff", 1e6));
            var ac = new AC("ac", new LinearSweep(1.0, 1.0, 1));
            var voltage = new ComplexPropertyExport(ac, "S1", "v");
            var current = new ComplexPropertyExport(ac, "S1", "i");
            var power = new ComplexPropertyExport(ac, "S1", "p");

            foreach (var _ in ac.Run(circuit, AC.ExportSmallSignal))
            {
                AssertComplex(current.Value, voltage.Value * 2.0);
                AssertComplex(power.Value, voltage.Value * Complex.Conjugate(current.Value));
            }
        }

        [Test]
        public void When_TransmissionLineExportsPower_Expect_PerPortValues()
        {
            var circuit = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0).SetParameter("acmag", 1.0),
                new Resistor("Rsource", "in", "a", 100.0),
                new LosslessTransmissionLine("T1", "a", "0", "b", "0", 50.0, 1e-3),
                new Resistor("Rload", "b", "0", 25.0));

            var op = new OP("op");
            var voltage2 = new RealPropertyExport(op, "T1", "v2");
            var current2 = new RealPropertyExport(op, "T1", "i2");
            var power2 = new RealPropertyExport(op, "T1", "p2");
            foreach (var _ in op.Run(circuit, OP.ExportOperatingPoint))
                Assert.That(power2.Value, Is.EqualTo(-voltage2.Value * current2.Value).Within(1e-12));

            var ac = new AC("ac", new LinearSweep(100.0, 100.0, 1));
            var complexVoltage1 = new ComplexPropertyExport(ac, "T1", "v1");
            var complexCurrent1 = new ComplexPropertyExport(ac, "T1", "i1");
            var complexPower1 = new ComplexPropertyExport(ac, "T1", "p1");
            var complexVoltage2 = new ComplexPropertyExport(ac, "T1", "v2");
            var complexCurrent2 = new ComplexPropertyExport(ac, "T1", "i2");
            var complexPower2 = new ComplexPropertyExport(ac, "T1", "p2");
            foreach (var _ in ac.Run(circuit, AC.ExportSmallSignal))
            {
                AssertComplex(complexPower1.Value, -complexVoltage1.Value * Complex.Conjugate(complexCurrent1.Value));
                AssertComplex(complexPower2.Value, -complexVoltage2.Value * Complex.Conjugate(complexCurrent2.Value));
            }
        }
    }
}