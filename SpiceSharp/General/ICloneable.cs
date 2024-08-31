namespace SpiceSharp
{
    /// <summary>
    /// Describes a cloneable item.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface ICloneable<T>
    {
        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>The cloned instance.</returns>
        T Clone();
    }
}
