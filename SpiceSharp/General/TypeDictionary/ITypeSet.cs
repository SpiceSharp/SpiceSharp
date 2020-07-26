namespace SpiceSharp
{
    /// <summary>
    /// An <see cref="ITypeDictionary{K, V}"/> that can add values by their own types.
    /// </summary>
    /// <typeparam name="V">The base value type.</typeparam>
    public interface ITypeSet<V> : ITypeDictionary<V, V>
    {
        /// <summary>
        /// Adds a value to the set.
        /// </summary>
        /// <param name="value">The value.</param>
        void Add(V value);
    }
}
