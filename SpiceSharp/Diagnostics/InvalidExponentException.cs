using SpiceSharp.Simulations;

namespace SpiceSharp.Diagnostics
{
    /// <summary>
    /// Exception thrown when two units are not matched.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class InvalidExponentException : SpiceSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnitsNotMatchedException"/> class.
        /// </summary>
        public InvalidExponentException(int numerator, int denominator)
            : base(Properties.Resources.Units_InvalidExponent.FormatString(numerator, denominator))
        {
        }
    }
}
