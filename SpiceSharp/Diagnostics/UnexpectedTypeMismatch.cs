using System;

namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when a type did not match.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class UnexpectedTypeMismatch : SpiceSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedTypeMismatch"/> class.
        /// </summary>
        /// <param name="expected">The expected type.</param>
        /// <param name="actual">The actual type.</param>
        public UnexpectedTypeMismatch(Type expected, Type actual)
            : base(Properties.Resources.UnexpectedType.FormatString(expected?.FullName, actual?.FullName))
        {
        }
    }
}
