namespace SpiceSharp.NewSparse
{
    /// <summary>
    /// Common interface for matrix element values
    /// </summary>
    public abstract class BaseElement
    {
        /// <summary>
        /// Gets the magnitude
        /// </summary>
        public abstract double Magnitude { get; }

        /// <summary>
        /// Negate the value
        /// </summary>
        public abstract void Negate();

        /// <summary>
        /// Multiply with a scalar
        /// </summary>
        /// <param name="factor">Scalar factor</param>
        public abstract void Scalar(double factor);

        /// <summary>
        /// Check if the value equals 1.0 or -1.0
        /// Used for finding identity multipliers in the matrix
        /// </summary>
        /// <returns></returns>
        public abstract bool EqualsOne();

        /// <summary>
        /// Check if the value equals 0.0
        /// </summary>
        /// <returns></returns>
        public abstract bool EqualsZero();
    }
}
