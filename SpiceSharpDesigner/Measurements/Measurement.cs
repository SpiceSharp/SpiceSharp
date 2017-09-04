using SpiceSharp.Simulations;

namespace SpiceSharp.Designer
{
    /// <summary>
    /// This class can perform a measurement
    /// </summary>
    public abstract class Measurement
    {
        /// <summary>
        /// Perform a measurement
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <returns></returns>
        public abstract double Measure(Circuit ckt);
    }

    /// <summary>
    /// Extract simulation data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public delegate double ExtractData(SimulationData data);

    /// <summary>
    /// Extract a parameter from the extract simulation data
    /// </summary>
    /// <param name="results">All simulation results</param>
    /// <returns></returns>
    public delegate double ExtractParameter<T>(T step) where T : Measurement;
}
