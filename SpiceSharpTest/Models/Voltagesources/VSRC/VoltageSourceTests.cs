using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models.Voltagesources.VSRC
{
    [TestFixture]
    public class VoltageSourceTests : Framework
    {
        [Test]
        public void When_VoltageSourceSeries_Expect_Reference()
        {
            double[] voltages = { 1.0, 1.5, 2.8, 3.9, 0.5, -0.1, -0.5 };

            // Build the circuit
            Circuit ckt = new Circuit();
            double sum = 0.0;
            for (int i = 0; i < voltages.Length; i++)
            {
                ckt.Objects.Add(new VoltageSource($"V{i + 1}", $"{i + 1}", $"{i}", voltages[i]));
                sum += voltages[i];
            }

            // Build the simulation, exports and references
            Op op = new Op("OP");
            Export<double>[] exports = { new RealVoltageExport(op, $"{voltages.Length}") };
            double[] references = { sum };
            AnalyzeOp(op, ckt, exports, references);
        }
    }
}
