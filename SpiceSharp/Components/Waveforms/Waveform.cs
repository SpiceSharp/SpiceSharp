using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Template for a waveform that can change value over time.
    /// </summary>
    /// <seealso cref="BaseParameter" />
    public abstract class Waveform : BaseParameter
    {
        /// <summary>
        /// Sets up the waveform.
        /// </summary>
        public abstract void Setup();

        /// <summary>
        /// Calculates the value of the waveform at a specific timepoint.
        /// </summary>
        /// <param name="time">The time point.</param>
        /// <returns>
        /// The value of the waveform.
        /// </returns>
        public abstract double At(double time);

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        /// <param name="simulation">The time-based simulation</param>
        public abstract void Accept(TimeSimulation simulation);

        /// <summary>
        /// Clones the parameter.
        /// </summary>
        /// <returns>
        /// The cloned parameter.
        /// </returns>
        public override BaseParameter Clone()
        {
            // 1. Make new object
            var destinationObject = (Waveform)Activator.CreateInstance(this.GetType());

            // 2. Copy properties of the current object
            Utility.CopyPropertiesAndFields(this, destinationObject);
            return destinationObject;
        }

        /// <summary>
        /// Copies the contents of a parameter to this parameter.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        public override void CopyFrom(BaseParameter source)
        {
            Utility.CopyPropertiesAndFields(source, this);
        }
    }
}
