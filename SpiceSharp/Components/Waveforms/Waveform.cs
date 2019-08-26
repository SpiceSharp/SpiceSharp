using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Template for a waveform that can change value over time.
    /// </summary>
    public abstract class Waveform : ICloneable, ICloneable<Waveform>
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

        /// <summary>
        /// Clones the waveform.
        /// </summary>
        /// <returns>
        /// The cloned waveform.
        /// </returns>
        public virtual Waveform Clone()
        {
            var destinationObject = (Waveform)Activator.CreateInstance(this.GetType());
            Reflection.CopyPropertiesAndFields(this, destinationObject);
            return destinationObject;
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns></returns>
        ICloneable ICloneable.Clone() => Clone();

        /// <summary>
        /// Copies the contents of another waveform to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        public virtual void CopyFrom(Waveform source)
        {
            Reflection.CopyPropertiesAndFields(source, this);
        }

        /// <summary>
        /// Copy the contents of another object to this one.
        /// </summary>
        /// <param name="source">The source object.</param>
        void ICloneable.CopyFrom(ICloneable source) => CopyFrom((Waveform)source);
    }
}
