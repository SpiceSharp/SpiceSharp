using System;

namespace SpiceSharp
{
    /// <summary>
    /// A template for a parameter that can be cloned and copied.
    /// </summary>
    /// <remarks>
    /// This class can be used to ensure that parameter sets are cloned correctly. This is to avoid issues when running
    /// multiple simulations in parallel where shared resources may be undesirable.
    /// </remarks>
    public interface ICloneable
    {
        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        ICloneable Clone();

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="source"/> does not have the same type.</exception>
        void CopyFrom(ICloneable source);
    }
}
