using System;

namespace SpiceSharp.ParameterSets
{
    /// <summary>
    /// An interface that describes a class or struct that defines a parameter set
    /// of the specified type.
    /// </summary>
    /// <typeparam name="P">The parameter set type.</typeparam>
    public interface IParameterized<out P> where P : IParameterSet, ICloneable<P>
    {
        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        P Parameters { get; }
    }
}
