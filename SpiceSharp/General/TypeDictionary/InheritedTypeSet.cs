namespace SpiceSharp.General
{
    /// <summary>
    /// An <see cref="InheritedTypeDictionary{K, V}"/> that can add values by their own types.
    /// </summary>
    /// <typeparam name="V">The base value type.</typeparam>
    public class InheritedTypeSet<V> : InheritedTypeDictionary<V, V>, ITypeSet<V>
    {

        /// <inheritdoc/>
        public void Add(V value) => Add(value.GetType(), value);
    }
}
