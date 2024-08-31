using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpTest.Models;
using System;

namespace SpiceSharpTest.Waveforms
{
    [TestFixture]
    public class PwlTests : Framework
    {
        [Test]
        public void When_PwlHasEmptyArray_Expect_Exception()
        {
            Assert.Throws<ArgumentException>(() => new Pwl() { Points = [] }.Create(null));
        }

        [Test]
        public void When_PwlHasNullArray_Expect_Exception()
        {
            Assert.Throws<ArgumentNullException>(() => new Pwl() { Points = null }.Create(null));
        }

        [Test]
        public void When_PwlHasNonMonotonouslyIncreasingTimePointsArray_Expect_Exception()
        {
            Assert.Throws<ArgumentException>(() => new Pwl() { Points = [new Point(1, 1.2), new Point(0.9, 1.3)] }.Create(null));
        }

        [Test]
        public void When_PwlHasOnePoint_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pwl() { Points = [new Point(1, 2)] }),
                new Resistor("R1", "in", "0", 10.0)
            );

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-6, 10.0);
            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                Assert.That(tran.GetVoltage("in"), Is.EqualTo(2.0).Within(1e-12));
            }
        }

        [Test]
        public void When_PwlHasTwoPoints_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pwl() { Points = [new Point(0, 0), new Point(1, 2)] }),
                new Resistor("R1", "in", "0", 10.0)
            );

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-6, 1.0);
            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                Assert.That(tran.GetVoltage("in"), Is.EqualTo(tran.Time < 1.0 ? 2.0 * tran.Time : 2.0).Within(1e-12));
            }
        }

        [Test]
        public void When_PwlHasMinusTimeValue_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pwl() { Points = [new Point(-1, 0), new Point(1, 2)] }),
                new Resistor("R1", "in", "0", 10.0)
            );

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-6, 1.0);
            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                Assert.That(tran.GetVoltage("in"), Is.EqualTo(tran.Time < 1.0 ? 1.0 + tran.Time : 2.0).Within(1e-12));
            }
        }

        [Test]
        public void When_PwlHasManyLinearPoints_Expect_Reference()
        {
            int n = 10000;
            var pts = new Point[n];

            for (int i = 0; i < n; i++)
                pts[i] = new Point(i, i);

            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pwl() { Points = pts }),
                new Resistor("R1", "in", "0", 10.0)
            );

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-2, n - 1);
            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                Assert.That(tran.GetVoltage("in"), Is.EqualTo(tran.Time).Within(1e-12));
            }
        }

        [Test]
        public void When_PwlHasManySawPoints_Expect_Reference()
        {
            int n = 10000;
            var pts = new Point[n];

            for (int i = 0; i < n; i++)
                pts[i] = new Point(i, i % 2);

            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pwl() { Points = pts }),
                new Resistor("R1", "in", "0", 10.0)
            );

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-2, n - 1);
            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                int integer = (int)tran.Time;

                if (tran.Time == integer)
                {
                    Assert.That(tran.GetVoltage("in"), Is.EqualTo(integer % 2).Within(1e-12));
                }
                else
                {
                    if (integer % 2 == 0)
                    {
                        Assert.That(tran.GetVoltage("in"), Is.EqualTo(tran.Time - integer).Within(1e-12));
                    }
                    else
                    {
                        Assert.That(tran.GetVoltage("in"), Is.EqualTo(1 - (tran.Time - integer)).Within(1e-12));
                    }
                }
            }
        }

        [Test]
        public void When_PwlHasTwoPositiveTimePointsTheyShouldBeHit_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pwl() { Points = [new Point(1.111, 2.0), new Point(3.34, 2.0)] }),
                new Resistor("R1", "in", "0", 10.0)
            );

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1.3e-6, 10.0);
            bool wasHit1 = false;
            bool wasHit2 = false;

            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                if (tran.Time == 1.111)
                {
                    wasHit1 = true;
                }

                if (tran.Time == 3.34)
                {
                    wasHit2 = true;
                }

                Assert.That(tran.GetVoltage("in"), Is.EqualTo(2.0).Within(1e-12));
            }

            Assert.That(wasHit1);
            Assert.That(wasHit2);
        }
    }
}
