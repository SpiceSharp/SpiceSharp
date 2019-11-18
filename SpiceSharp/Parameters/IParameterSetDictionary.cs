namespace SpiceSharp
{
    /// <summary>
    /// Interface describing a set of parameters.
    /// </summary>
    /// <seealso cref="ICloneable" />
    /// <seealso cref="ITypeDictionary{T}" />
    public interface IParameterSetDictionary : ICloneable, ITypeDictionary<IParameterSet>, INamedParameterCollection
    {
    }
}
