using System;

namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when a value type could not be found.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class ValueNotFoundException : SpiceSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueNotFoundException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public ValueNotFoundException(Type type)
            : base(Properties.Resources.ValueNotFound.FormatString(type.FullName))
        {
        }
    }
}
