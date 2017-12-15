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
            // After 5 * tau = 5 * R*C, voltage on capacitor should be about 99.3% of DC voltage

            double dcVoltage = 10;
            double resistorResistance = 10000;
            double capacitance = 0.000001;

            double tau = resistorResistance * capacitance;

            Circuit ckt = new Circuit();

            var capacitor = new SpiceSharp.Components.Capacitor(
                new Identifier("C_1"),
                new Identifier("OUT"),
                new Identifier("gnd"),
                capacitance
            );
            
            ckt.Objects.Add(
                new Voltagesource(
                    new Identifier("V_1"),
                    new Identifier("IN"),
                    new Identifier("gnd"),
                    dcVoltage),
                new SpiceSharp.Components.Resistor(
                    new Identifier("R_1"),
                    new Identifier("IN"),
                    new Identifier("OUT"),
                    resistorResistance),
                capacitor
            );
            ckt.Method = new Trapezoidal();

            double maxVoltage = 0;
            Transient trans = new Transient("T", 0.001, 5 * tau);
            trans.Circuit = ckt; //TODO: refactor this ..
            ckt.Simulation = trans; //TODO: refactor this ..
            // trans.CurrentConfig.UseIC = true;
            ckt.Nodes.IC.Add("OUT", 0.0);
            trans.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                var outVoltage = data.GetVoltage(new Identifier("OUT"), new Identifier("gnd"));
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
