namespace SpiceSharp
{
    /// <summary>
    /// A template for a parameter that can be cloned and copied.
    /// </summary>
    /// <remarks>
    /// This class is used to ensure that parameter sets are cloned correctly. This is to avoid issues when running
    /// multiple simulations in parallel where shared memory may be undesirable.
    /// </remarks>
    public interface IDeepCloneable
    {
        /// <summary>
        /// Clones the parameter.
        /// </summary>
        /// <returns>
        /// The cloned parameter.
        /// </returns>
        IDeepCloneable Clone();

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        void CopyFrom(IDeepCloneable source);
    }
}
