using SpiceSharp.Simulations;

namespace SpiceSharp.Designer
{
    /// <summary>
    /// A DC measurement
    /// </summary>
    public class OPMeasurement : Measurement
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private ExtractData extract;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="extract">The extraction method</param>
        public OPMeasurement(ExtractData extract)
        {
            this.extract = extract;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">The positive output node</param>
        /// <param name="reference">The negative output node</param>
        public OPMeasurement(string node, string reference = null)
        {
            extract = (SimulationData data) => data.GetVoltage(node, reference);
        }

        /// <summary>
        /// Measure
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override double Measure(Circuit ckt)
        {
            OP op = new OP("OPMeasurement");
            double result = double.NaN;
            op.OnExportSimulationData += (object sender, SimulationData data) => result = extract(data);
            ckt.Simulate(op);
            return result;
        }
    }
}
