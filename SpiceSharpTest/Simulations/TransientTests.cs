using System;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class TransientTests
    {
        [Test]
        public void When_RCFilterConstantTransient_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10),
                new Capacitor("C1", "out", "0", 20)
            );

            // Create the transient analysis
            var tran = new Transient("tran 1", 1.0, 10.0);
            tran.ParameterSets.Get<TimeConfiguration>().InitTime = 0.0;
            tran.ParameterSets.Get<TimeConfiguration>().Method = new Gear();
            tran.OnExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(args.GetVoltage("out"), 10.0, 1e-12);
            };
            tran.Run(ckt);

            // Let's run the simulation twice to check if it is consistent
            try
            {
                tran.Run(ckt);
            }
            catch (Exception)
            {
                throw new Exception(@"Cannot run transient analysis twice");
            }
        }

        [Test]
        public void When_RCFilterConstantTransientGear_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10),
                new Capacitor("C1", "out", "0", 20));

            // Create the transient analysis
            var tran = new Transient("tran 1", 1.0, 10.0);
            tran.OnExportSimulationData += (sender, args) => { Assert.AreEqual(args.GetVoltage("out"), 10.0, 1e-12); };
            tran.Run(ckt);

            // Let's run the simulation twice to check if it is consistent
            try
            {
                tran.Run(ckt);
            }
            catch (Exception)
            {
                throw new Exception(@"Cannot run transient analysis twice");
            }
        }

        [Test]
        public void When_FloatingRTransient_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10.0)
            );

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-6, 10.0);
            tran.OnExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(args.GetVoltage("out"), 10.0, 1e-12);
            };
            tran.Run(ckt);
        }
    }
}
