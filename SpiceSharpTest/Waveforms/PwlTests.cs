using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Components.Waveforms;
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
            Assert.Throws<ArgumentException>(() => new Pwl(new double[] { }, new double[] { }));
        }

        [Test]
        public void When_PwlHasNullArray_Expect_Exception()
        {
            Assert.Throws<ArgumentNullException>(() => new Pwl(null, new double[] { }));
            Assert.Throws<ArgumentNullException>(() => new Pwl(new double[] { 1.0 }, null));
        }

        [Test]
        public void When_PwlHasNonMonotonouslyIncreasingTimePointsArray_Expect_Exception()
        {
            Assert.Throws<ArgumentException>(() => new Pwl(new double[] { 1.0, 0.9 }, new double[] { 1.2, 1.3 }));
        }

        [Test]
        public void When_PwlHasOnePoint_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pwl(new double[] { 1.0 }, new double[] { 2.0 })),
                new Resistor("R1", "in", "0", 10.0)
            );

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-6, 10.0);
            tran.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(2.0, args.GetVoltage("in"), 1e-12);
            };
            tran.Run(ckt);
        }

        [Test]
        public void When_PwlHasTwoPoints_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pwl(new double[] { 0.0, 1.0 }, new double[] { 0.0, 2.0 })),
                new Resistor("R1", "in", "0", 10.0)
            );

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-6, 1.0);
            tran.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(args.Time < 1.0 ? 2.0 * args.Time : 2.0, args.GetVoltage("in"), 1e-12);
            };
            tran.Run(ckt);
        }

        [Test]
        public void When_PwlHasMinusTimeValue_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pwl(new double[] { -1.0, 1.0 }, new double[] { 0.0, 2.0 })),
                new Resistor("R1", "in", "0", 10.0)
            );

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-6, 1.0);
            tran.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(args.Time < 1.0 ? 1.0 + args.Time : 2.0, args.GetVoltage("in"), 1e-12);
            };
            tran.Run(ckt);
        }

        [Test]
        public void When_PwlHasManyLinearPoints_Expect_Reference()
        {
            int n = 10000;
            var times = new double[n];
            var voltages = new double[n];

            for (var i = 0; i < n; i++)
            {
                times[i] = i;
                voltages[i] = i;
            }

            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pwl(times, voltages)),
                new Resistor("R1", "in", "0", 10.0)
            );

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-2, n - 1);
            tran.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(args.Time, args.GetVoltage("in"), 1e-12);
            };
            tran.Run(ckt);
        }

        [Test]
        public void When_PwlHasManySawPoints_Expect_Reference()
        {
            int n = 10000;
            var times = new double[n];
            var voltages = new double[n];

            for (var i = 0; i < n; i++)
            {
                times[i] = i;
                voltages[i] = i % 2;
            }

            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pwl(times, voltages)),
                new Resistor("R1", "in", "0", 10.0)
            );

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-2, n - 1);
            tran.ExportSimulationData += (sender, args) =>
            {
                int integer = (int)args.Time;

                if (args.Time == integer)
                {
                    Assert.AreEqual(integer % 2, args.GetVoltage("in"), 1e-12);
                }
                else
                {
                    if (integer % 2 == 0)
                    {
                        Assert.AreEqual(args.Time - integer, args.GetVoltage("in"), 1e-12);
                    }
                    else
                    {
                        Assert.AreEqual(1 - (args.Time - integer), args.GetVoltage("in"), 1e-12);
                    }
                }
            };
            tran.Run(ckt);
        }

        [Test]
        public void When_PwlHasTwoPositiveTimePointsTheyShouldBeHitted_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pwl(new double[] { 1.111, 3.34 }, new double[] { 2.0, 2.0 })),
                new Resistor("R1", "in", "0", 10.0)
            );

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1.3e-6, 10.0);
            bool wasHit1 = false;
            bool wasHit2 = false;

            tran.ExportSimulationData += (sender, args) =>
            {
                if (args.Time == 1.111)
                {
                    wasHit1 = true;
                }

                if (args.Time == 3.34)
                {
                    wasHit2 = true;
                }

                Assert.AreEqual(2.0, args.GetVoltage("in"), 1e-12);
            };
            tran.Run(ckt);

            Assert.True(wasHit1);
            Assert.True(wasHit2);
        }
    }
}
