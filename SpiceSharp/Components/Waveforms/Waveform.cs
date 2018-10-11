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
        /// Gets the current waveform value at the last probed timepoint.
        /// </summary>
        /// <value>
        /// The waveform value.
        /// </value>
        public virtual double Value { get; protected set; }
        
        /// <summary>
        /// Sets up the waveform.
        /// </summary>
        public abstract void Setup();

        /// <summary>
        /// Indicates a new timepoint is being probed.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public abstract void Probe(TimeSimulation simulation);

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
