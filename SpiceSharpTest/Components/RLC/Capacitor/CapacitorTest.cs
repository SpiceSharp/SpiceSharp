using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using SpiceSharpTest.Utils;

namespace SpiceSharpTest.Components.RLC.Capacitor
{
    [TestClass]
    public class CapacitorTest
    {
        [TestMethod]
        public void Circuit_RC_Transient()
        {
            // A test for a RC circuit (DC voltage, resistor, capacitor)
            // An init voltage on capacitor is 0V
            // After 5 * tau = 5 * R*C, voltage on capacitor should be 99.7% of DC voltage

            double dcVoltage = 10;
            double resistorResistance = 10000;
            double capacitance = 0.000001;

            double tau = resistorResistance * capacitance;

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
                    resistorResistance),
                capacitor
            );
            ckt.Method = new Trapezoidal();

            double maxVoltage = 0;
            Transient trans = new Transient("T", 0.001, 5 * tau);
            trans.Circuit = ckt; //TODO: refactor this ..
            ckt.Simulation = trans; //TODO: refactor this ..
            trans.CurrentConfig.UseIC = true;
            trans.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                var outVoltage = data.GetVoltage(new CircuitIdentifier("OUT"), new CircuitIdentifier("gnd"));
                if (outVoltage > maxVoltage)
                {
                    maxVoltage = outVoltage;
                }
             };
            trans.SetupAndExecute();

            Assert.That.AreEqualWithTol(9.93, maxVoltage, 0, 0.01);

        }
    }
}
