namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Parameters for a <see cref="FrequencySimulation"/>
    /// </summary>
    public class FrequencyConfiguration : ParameterSet
    {
        /// <summary>
        /// Keep operating point information
        /// </summary>
        public bool KeepOpInfo { get; set; } = false;

        /// <summary>
        /// The sweep used for the frequency
        /// </summary>
        public Sweep<double> FrequencySweep { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public FrequencyConfiguration()
        {
            // Default frequency-sweep
            FrequencySweep = new Sweeps.DecadeSweep(1, 100, 10);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="frequencySweep">Sweep used for the frequency points</param>
        public FrequencyConfiguration(Sweep<double> frequencySweep)
        {
            FrequencySweep = frequencySweep;
        }
    }
}
