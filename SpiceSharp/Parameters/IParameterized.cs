namespace SpiceSharp.ParameterSets
{
    /// <summary>
    /// An interface that describes a class or struct that defines a parameter set
    /// of the specified type.
    /// </summary>
    /// <typeparam name="P">The parameter set type.</typeparam>
    /// <remarks>
    /// This interface allows anyone to check whether a class defines a parameter set of the given type, e.g. for
    /// <see cref="IParameterSetCollection.TryGetParameterSet{P}(out P)"/>.
    /// </remarks>
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
