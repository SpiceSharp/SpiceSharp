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
        public void When_Pwl_Has_Empty_Array_Expect_Exception()
        {
            Assert.Throws<ArgumentException>( () => new Pwl(new double[] { }, new double[] { }));
        }

        [Test]
        public void When_Pwl_Has_Null_Array_Expect_Exception()
        {
            Assert.Throws<ArgumentNullException>(() => new Pwl(null, new double[] { }));
            Assert.Throws<ArgumentNullException>(() => new Pwl(new double[] { }, null));
        }

        [Test]
        public void When_Pwl_Has_One_Value()
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
        public void When_Pwl_Has_Two_Values()
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
        public void When_Pwl_Has_Minus_Time_Value()
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
        public void When_Pwl_Has_Many_Values()
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
                Assert.AreEqual(args.Time < n ? args.Time : n, args.GetVoltage("in"), 1e-12);
            };
            tran.Run(ckt);
        }
    }
}
