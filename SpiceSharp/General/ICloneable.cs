namespace SpiceSharp
{
    /// <summary>
    /// A template for a parameter that can be cloned and copied.
    /// </summary>
    /// <remarks>
    /// This class can be used to ensure that parameter sets are cloned correctly. This is to avoid issues when running
    /// multiple simulations in parallel where shared memory may be undesirable.
    /// </remarks>
    public interface ICloneable
    {
        /// <summary>
        /// Clones the parameter.
        /// </summary>
        /// <returns>
        /// The cloned parameter.
        /// </returns>
        ICloneable Clone();

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        void CopyFrom(ICloneable source);
    }

    /// <summary>
    /// A template for a parameter that can be cloned and copied (typed version).
    /// </summary>
    /// <remarks>
    /// This class can be used to ensure that parameter sets are cloned correctly. This is to avoid issues when running
    /// multiple simulations in parallel where shared memory may be undesirable.
    /// </remarks>
    public interface ICloneable<T>
    {
        /// <summary>
        /// Clones the parameter.
        /// </summary>
        /// <returns>
        /// The cloned parameter.
        /// </returns>
        T Clone();

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        void CopyFrom(T source);
    }
}
