namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A configuration for a <see cref="FrequencySimulation" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class FrequencyConfiguration : ParameterSet
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation point should be exported.
        /// </summary>
        public bool KeepOpInfo { get; set; } = false;

        /// <summary>
        /// Gets or sets the absolute threshold for choosing pivots.
        /// </summary>
        public double AbsolutePivotThreshold { get; set; }

        /// <summary>
        /// Gets or sets the relative threshold for choosing pivots.
        /// </summary>
        public double RelativePivotThreshold { get; set; } = 1e-3;

        /// <summary>
        /// Gets or sets the frequency sweep.
        /// </summary>
        public Sweep<double> FrequencySweep { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyConfiguration"/> class.
        /// </summary>
        public FrequencyConfiguration()
        {
            // Default frequency-sweep
            FrequencySweep = new DecadeSweep(1, 100, 10);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyConfiguration"/> class.
        /// </summary>
        /// <param name="frequencySweep">The frequency sweep.</param>
        public FrequencyConfiguration(Sweep<double> frequencySweep)
        {
            FrequencySweep = frequencySweep;
        }
    }
}
