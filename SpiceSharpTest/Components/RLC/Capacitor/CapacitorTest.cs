using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Components.RLC.Capacitor
{
    [TestClass]
    public class CapacitorTest
    {
        [TestMethod]
        public void Circuit_RC_Transient()
        {
            double dcVoltage = 10;
            double resistorResitance = 10000;
            double capacitance = 0.000001;

            double tau = resistorResitance * capacitance;

            Circuit ckt = new Circuit();

            var capacitor = new SpiceSharp.Components.Capacitor(
                new CircuitIdentifier("C_1"),
                new CircuitIdentifier("OUT"),
                new CircuitIdentifier("gnd"),
                capacitance
            );
            
            ckt.Objects.Add(
                new Voltagesource(
                    new CircuitIdentifier("V_1"),
                    new CircuitIdentifier("IN"),
                    new CircuitIdentifier("gnd"),
                    dcVoltage),
                new SpiceSharp.Components.Resistor(
                    new CircuitIdentifier("R_1"),
                    new CircuitIdentifier("IN"),
                    new CircuitIdentifier("OUT"),
                    resistorResitance),
                capacitor
            );
            capacitor.CAPinitCond.Set(10);
            ckt.Method = new Trapezoidal();

            Transient trans = new Transient("T", 0.000001, 5 * tau);
            trans.Circuit = ckt; //TODO: refactor this ..
            ckt.Simulation = trans; //TODO: refactor this ..
            trans.CurrentConfig.UseIC = true;
            trans.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                var outVoltage = data.GetVoltage(new CircuitIdentifier("OUT"), new CircuitIdentifier("gnd"));
                Console.WriteLine("Out voltage: " + outVoltage);
            };
            trans.SetupAndExecute();
        }
    }
}
