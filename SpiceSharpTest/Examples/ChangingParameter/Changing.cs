using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Examples
{
    [TestFixture]
    public class Changing
    {
        [Test]
        public void When_ChangeParameterInTransient_Expect_NoException()
        {
            // <example_change_parameter_circuit>
            // Build a circuit
            var ckt = new Circuit(
                new Resistor("R1", "in", "out", 1.0e3),
                new Resistor("R2", "out", "0", 1.0e3),
                new Capacitor("C1", "out", "0", 0.5e-9),
                new VoltageSource("V1", "in", "0", new Pulse(0, 5, 1e-6, 1e-6, 1e-6, 1e-5, 2e-5))
            );
            // </example_change_parameter_circuit>
            // <example_change_parameter_transient>
            // Create the transient analysis and exports
            var tran = new Transient("tran", 1e-6, 10e-5);
            var outputExport = new RealVoltageExport(tran, "out");

            // </example_change_parameter_transient>
            // <example_change_parameter_setup>
            // Now we need to make sure we have a reference to both the base parameters and temperature behavior
            // of the resistor
            SpiceSharp.Components.Resistors.Parameters bp = null;
            SpiceSharp.Behaviors.ITemperatureBehavior tb = null;
            // </example_change_parameter_setup>
            // <example_change_parameter_load>
            // Before loading the resistor, let's change its value first!
            tran.BeforeLoad += (sender, args) =>
            {
                // First we need to figure out the timepoint that will be loaded
                double time = tran.GetState<IIntegrationMethod>().Time;

                // Then we need to calculate the resistance for "R2"
                double resistance = 1.0e3 * (1 + time * 1.0e5);

                // Now let's update the parameter
                bp.Resistance = resistance;
                tb.Temperature();
            };

            // Run the simulation
            foreach (int status in tran.Run(ckt, Simulation.AfterSetup | Simulation.Exports))
            {
                switch (status)
                {
                    case Simulation.AfterSetup:
                        var eb = tran.EntityBehaviors["R2"];
                        eb.TryGetValue(out tb);
                        eb.TryGetParameterSet(out bp);
                        break;

                    default:
                        double time = tran.Time;
                        double output = outputExport.Value;
                        break;
                }
            }
            // </example_change_parameter_load>
        }
    }
}