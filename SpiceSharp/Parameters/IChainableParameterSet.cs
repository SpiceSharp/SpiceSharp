namespace SpiceSharp
{
    /// <summary>
    /// Describes an interface that can chain parameter method calls.
    /// </summary>
    /// <typeparam name="T">The class type that will be chained.</typeparam>
    /// <seealso cref="IParameterSet" />
    public interface IChainableParameterSet<T> : IImportParameterSet
    {
        /// <summary>
        /// Call a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <returns>The current instance for chaining.</returns>
        new T SetParameter(string name);

        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current instance for chaining.</returns>
        new T SetParameter<P>(string name, P value);
    }
}
