namespace SpiceSharp.NewSparse
{
    /// <summary>
    /// Common interface for matrix element values
    /// </summary>
    public interface IElement
    {
        /// <summary>
        /// Gets the magnitude
        /// </summary>
        double Magnitude { get; }
    }
}
