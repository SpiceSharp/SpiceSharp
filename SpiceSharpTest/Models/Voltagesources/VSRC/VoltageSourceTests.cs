using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class VoltageSourceTests : Framework
    {
        [Test]
        public void When_VoltageSourceSeries_Expect_Reference()
        {
            double[] voltages = { 1.0, 1.5, 2.8, 3.9, 0.5, -0.1, -0.5 };

            // Build the circuit
            var ckt = new Circuit();
            var sum = 0.0;
            for (var i = 0; i < voltages.Length; i++)
            {
                ckt.Add(new VoltageSource($"V{i + 1}", $"{i + 1}", $"{i}", voltages[i]));
                sum += voltages[i];
            }

            // Build the simulation, exports and references
            var op = new OP("OP");
            Export<double>[] exports = { new RealVoltageExport(op, $"{voltages.Length}") };
            double[] references = { sum };
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }
    }
}
