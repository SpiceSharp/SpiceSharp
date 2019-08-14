using System;
using SpiceSharp.Attributes;

namespace SpiceSharp
{
    /// <summary>
    /// A class that describes a set of parameters.
    /// </summary>
    /// <remarks>
    /// This class allows accessing parameters by their metadata. Metadata is specified by using 
    /// the <see cref="ParameterNameAttribute"/> and <see cref="ParameterInfoAttribute"/>.
    /// </remarks>
    public abstract class ParameterSet : ICloneable, ICloneable<ParameterSet>
    {
        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public virtual void CalculateDefaults()
        {
            // By default, there are no parameter values that depend on others
        }

        /// <summary>
        /// Creates a clone of the parameter set.
        /// </summary>
        /// <returns>
        /// A clone of the parameter set.
        /// </returns>
        public virtual ParameterSet Clone()
        {
            var clone = (ParameterSet) Activator.CreateInstance(GetType());
            clone.CopyFrom(this);
            return clone;
        }

        /// <summary>
        /// Creates a clone of the parameter set.
        /// </summary>
        /// <returns>
        /// A clone of the parameter set.
        /// </returns>
        ICloneable ICloneable.Clone() => Clone();

        /// <summary>
        /// Copy properties and fields from another parameter set.
        /// </summary>
        /// <param name="source">The source parameter set.</param>
        public virtual void CopyFrom(ParameterSet source)
        {
            Reflection.CopyPropertiesAndFields(source, this);
        }

        /// <summary>
        /// Copy parameters from another object.
        /// </summary>
        /// <param name="source">The source object.</param>
        void ICloneable.CopyFrom(ICloneable source) => CopyFrom((ParameterSet)source);
    }
}
