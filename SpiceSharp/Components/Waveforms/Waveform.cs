using SpiceSharp.Entities;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Template for a waveform that can change value over time.
    /// </summary>
    public abstract class Waveform : ParameterSet
    {
        /// <summary>
        /// Gets the current waveform value at the last probed timepoint.
        /// </summary>
        public virtual double Value { get; protected set; }

        /// <summary>
        /// Binds the waveform to the simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public abstract void Bind(BindingContext context);

        /// <summary>
        /// Probes a new timepoint.
        /// </summary>
        public abstract void Probe();

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        public abstract void Accept();
    }
}
