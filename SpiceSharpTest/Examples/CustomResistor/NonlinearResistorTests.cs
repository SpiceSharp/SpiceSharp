using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Examples
{
    [TestFixture]
    public class NonlinearResistorTests
    {
        [Test]
        public void When_RunNonlinearResistor_Expect_NoException()
        {
            // <example_customcomponent_nonlinearresistor_test>
            // Build the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "out", "0", 0.0),
                new NonlinearResistor("RNL1", "out", "0")
                    .SetParameter("a", 2.0e3)
                    .SetParameter("b", 0.5)
            );

            // Setup the simulation and export our current
            var dc = new DC("DC", "V1", -2.0, 2.0, 1e-2);
            var currentExport = new RealPropertyExport(dc, "V1", "i");
            foreach (int _ in dc.Run(ckt))
            {
                double current = -currentExport.Value;
                System.Console.Write("{0}, ".FormatString(current));
            }
            // </example_customcomponent_nonlinearresistor_test>
        }
    }
}
