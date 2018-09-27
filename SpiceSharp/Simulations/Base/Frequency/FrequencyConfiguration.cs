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
        /// <value>
        ///   <c>true</c> if the operating point should be exported; otherwise, <c>false</c>.
        /// </value>
        public bool KeepOpInfo { get; set; } = false;

        /// <summary>
        /// Gets or sets the frequency sweep.
        /// </summary>
        /// <value>
        /// The frequency sweep.
        /// </value>
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
