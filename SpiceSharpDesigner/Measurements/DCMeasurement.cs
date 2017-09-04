using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharp.Designer
{
    public class DCMeasurement : Measurement
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private ExtractData extractsim;
        private ExtractParameter<DCMeasurement> extractparam;

        /// <summary>
        /// Gets the simulated frequencies
        /// </summary>
        public List<double>[] Sweeps { get; private set; } = null;

        /// <summary>
        /// Get the measured quantities
        /// </summary>
        public List<double> Results { get; } = new List<double>();

        /// <summary>
        /// Get the analysis
        /// </summary>
        public DC Analysis { get; } = new DC("DCMeasurement");

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="extractsim">Method for getting a data point</param>
        /// <param name="extractparam">Method for extracting the parameter from the data</param>
        public DCMeasurement(ExtractData extractsim, ExtractParameter<DCMeasurement> extractparam)
        {
            this.extractsim = extractsim;
            this.extractparam = extractparam;
        }

        /// <summary>
        /// Measure
        /// </summary>
        /// <param name="ckt"></param>
        /// <returns></returns>
        public override double Measure(Circuit ckt)
        {
            Sweeps = new List<double>[Analysis.Sweeps.Count];
            Analysis.OnExportSimulationData += StoreResult;
            ckt.Simulate(Analysis);
            Analysis.OnExportSimulationData -= StoreResult;
            return extractparam(this);
        }

        /// <summary>
        /// Store the result of the AC simulation
        /// </summary>
        /// <param name="sender">Simulation invoking the event</param>
        /// <param name="data">Simulation data</param>
        private void StoreResult(object sender, SimulationData data)
        {
            for (int i = 0; i < Analysis.Sweeps.Count; i++)
                Sweeps[i].Add(Analysis.Sweeps[i].CurrentValue);
            Results.Add(extractsim(data));
        }
    }
}
