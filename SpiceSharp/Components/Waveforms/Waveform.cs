using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Provides values in function of time. This is an abstract class.
    /// </summary>
    public abstract class Waveform : BaseParameter
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

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>
        /// A clone of the object.
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
        /// Copy from another object
        /// </summary>
        /// <param name="source">Source</param>
        public override void CopyFrom(BaseParameter source)
        {
            Utility.CopyPropertiesAndFields(source, this);
        }
    }
}
