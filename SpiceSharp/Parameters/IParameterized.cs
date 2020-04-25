using System;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// Describes a class or struct that contains parameter sets. These parameter sets can then
    /// be retrieved by their type, rather than by a property. No other knowledge about the instance
    /// is needed.
    /// </summary>
    public interface IParameterized
    {
        /// <summary>
        /// Gets the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <returns>
        /// The parameter set.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the parameter set could not be found.</exception>
        P GetParameterSet<P>() where P : IParameterSet;

        /// <summary>
        /// Tries to get the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <param name="value">The parameter set.</param>
        /// <returns>
        /// <c>true</c> if the parameter set was found; otherwise, <c>false</c>.
        /// </returns>
        bool TryGetParameterSet<P>(out P value) where P : IParameterSet;

        /// <summary>
        /// Gets all the parameter sets of this instance.
        /// </summary>
        /// <value>
        /// The parameter sets.
        /// </value>
        IEnumerable<IParameterSet> ParameterSets { get; }
    }

    /// <summary>
    /// A contract that a class or struct contains a parameter set of a specified type.
    /// </summary>
    /// <typeparam name="P">The parameter set type.</typeparam>
    public interface IParameterized<out P> : IParameterized where P : IParameterSet
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
