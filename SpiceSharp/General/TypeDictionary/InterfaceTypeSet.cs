namespace SpiceSharp.General
{
    /// <summary>
    /// An <see cref="InterfaceTypeDictionary{K, V}"/> that can add values by their own types.
    /// </summary>
    /// <typeparam name="V">The base value type.</typeparam>
    public class InterfaceTypeSet<V> : InterfaceTypeDictionary<V, V>, ITypeSet<V>
    {
        /// <inheritdoc/>
        public void Add(V value) => Add(value.GetType(), value);
    }
}
