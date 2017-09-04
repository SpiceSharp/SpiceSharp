using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharp.Designer
{
    public class ACMeasurement : Measurement
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private ExtractData extractsim;
        private ExtractParameter<ACMeasurement> extractparam;

        /// <summary>
        /// Gets the simulated frequencies
        /// </summary>
        public List<double> Frequencies { get; } = new List<double>();

        /// <summary>
        /// Get the measured quantities
        /// </summary>
        public List<double> Results { get; } = new List<double>();

        /// <summary>
        /// Get the analysis that will be run for extraction
        /// </summary>
        public AC Analysis { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="extract">The method for extracting our parameter</param>
        public ACMeasurement(ExtractData extractsim, ExtractParameter<ACMeasurement> extractparam)
        {
            this.extractsim = extractsim ?? throw new ArgumentNullException(nameof(extractsim));
            this.extractparam = extractparam ?? throw new ArgumentNullException(nameof(extractparam));
            Analysis = new AC("ACMeasurement");
        }

        /// <summary>
        /// Measure the AC output
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override double Measure(Circuit ckt)
        {
            // Initialize
            Frequencies.Clear();
            Results.Clear();

            // Simulate
            Analysis.OnExportSimulationData += StoreResult;
            ckt.Simulate(Analysis);
            Analysis.OnExportSimulationData -= StoreResult;

            // Return result
            return extractparam(this);
        }

        /// <summary>
        /// Store the result of the AC simulation
        /// </summary>
        /// <param name="sender">Simulation invoking the event</param>
        /// <param name="data">Simulation data</param>
        private void StoreResult(object sender, SimulationData data)
        {
            Frequencies.Add(data.GetFrequency());
            Results.Add(extractsim(data));
        }

        /// <summary>
        /// Find the frequency for a given result value
        /// This method will interpolate if necessary. Set <paramref name="logarithmic"/> to true
        /// if you wish to interpolate in log frequency axis.
        /// </summary>
        /// <param name="y">The output</param>
        /// <param name="logarithmic">Default is true</param>
        /// <returns></returns>
        public double FindAtY(double y, bool logarithmic = true)
        {
            // Find the index where our results cross y
            int index = 0;
            if (Results[0] > y)
            {
                while (index < Results.Count && Results[index] > y)
                    index++;
            }
            else if (Results[0] < y)
            {
                while (index < Results.Count && Results[index] < y)
                    index++;
            }
            else
                return Frequencies[0];
            if (index == Results.Count)
                throw new DesignException($"Cannot interpolate frequency");

            // Interpolate for finding the frequency
            if (logarithmic)
            {
                if (Frequencies[index - 1] <= 0.0 || Frequencies[index] <= 0.0)
                    throw new DesignException($"Cannot interpolate logarithmically using {Frequencies[index - 1]} Hz and {Frequencies[index]} Hz");
                double m = (Results[index - 1] - Results[index]) / Math.Log(Frequencies[index - 1] / Frequencies[index]);
                double logf = (y - Results[index]) / m + Math.Log(Frequencies[index]);
                return Math.Exp(logf);
            }
            else
            {
                double m = (Results[index - 1] - Results[index]) / (Frequencies[index - 1] - Frequencies[index]);
                return (y - Results[index]) / m + Frequencies[index];
            }
        }
    }
}
