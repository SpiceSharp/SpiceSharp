using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Provides values in function of time. This is an abstract class.
    /// </summary>
    public abstract class Waveform
    {
        /// <summary>
        /// Setup the waveform
        /// </summary>
        public abstract void Setup();

        /// <summary>
        /// Calculate the value of the waveform at a specific value
        /// </summary>
        /// <param name="time">The time point</param>
        /// <returns></returns>
        public abstract double At(double time);

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public abstract void Accept(TimeSimulation simulation);
    }
}
