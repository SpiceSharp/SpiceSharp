namespace SpiceSharp
{
    /// <summary>
    /// An interface describing a class that contains parameter sets.
    /// </summary>
    /// <seealso cref="IChainableImportParameterSet{T}" />
    /// <seealso cref="IParameterSet" />
    public interface IParameterized<T> :
        IChainableImportParameterSet<T>, IParameterSet
    {
        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        IParameterSetDictionary Parameters { get; }
    }
}
