namespace SpiceSharp
{
    /// <summary>
    /// A template for a parameter that can be used in parameter sets.
    /// </summary>
    /// <remarks>
    /// Use this class to ensure that parameter sets are cloned correctly to avoid issues when running
    /// multiple simulations in parallel.
    /// </remarks>
    public abstract class BaseParameter
    {
        /// <summary>
        /// Clones the parameter.
        /// </summary>
        /// <returns>
        /// The cloned parameter.
        /// </returns>
        public abstract BaseParameter Clone();

        /// <summary>
        /// Copies the contents of a parameter to this parameter.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        public abstract void CopyFrom(BaseParameter source);
    }
}
