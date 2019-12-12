namespace SpiceSharp.Components
{
    /// <summary>
    /// An instance of a waveform that can be used to sample datapoints.
    /// </summary>
    public interface IWaveform
    {
        /// <summary>
        /// Gets the value that is currently being probed.
        /// </summary>
        /// <value>
        /// The value at the probed timepoint.
        /// </value>
        double Value { get; }

        /// <summary>
        /// Probes a new timepoint.
        /// </summary>
        void Probe();

        /// <summary>
        /// Accepts the last probed timepoint.
        /// </summary>
        void Accept();
    }
}
