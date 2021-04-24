using System.Collections.Generic;

namespace SpiceSharp.ParameterSets
{
    /// <summary>
    /// Describes a class or struct that contains multiple parameter sets. These parameter sets can then
    /// be retrieved by their type.
    /// </summary>
    /// <remarks>
    /// A parameter set collection should generally not contain parameter sets that define parameters or properties with
    /// identical names. Depending on the implementation it may result in some parameters not being set, or some
    /// properties becoming inaccessible via the interface methods.
    /// </remarks>
    /// <seealso cref="IParameterSet"/>
    public interface IParameterSetCollection : IParameterSet
    {
        /// <summary>
        /// Gets the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <returns>
        /// The parameter set.
        /// </returns>
        /// <exception cref="TypeNotFoundException">Thrown if the parameter set could not be found.</exception>
        P GetParameterSet<P>() where P : IParameterSet, ICloneable<P>;

        /// <summary>
        /// Tries to get the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <param name="value">The parameter set.</param>
        /// <returns>
        ///   <c>true</c> if the parameter set was found; otherwise, <c>false</c>.
        /// </returns>
        bool TryGetParameterSet<P>(out P value) where P : IParameterSet, ICloneable<P>;

        /// <summary>
        /// Gets all the parameter sets of this instance.
        /// </summary>
        /// <value>
        /// The parameter sets.
        /// </value>
        IEnumerable<IParameterSet> ParameterSets { get; }
    }
}
