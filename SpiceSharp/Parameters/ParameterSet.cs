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
    /// <seealso cref="NamedParameterized" />
    public abstract class ParameterSet
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
        /// Creates a deep clone of the parameter set.
        /// </summary>
        /// <returns>
        /// A deep clone of the parameter set.
        /// </returns>
        public virtual ParameterSet DeepClone()
        {
            // 1. Make new object
            var destinationObject = (ParameterSet) Activator.CreateInstance(GetType());

            // 2. Copy properties of the current object
            ParameterHelper.CopyPropertiesAndFields(this, destinationObject);
            return destinationObject;
        }
    }
}
