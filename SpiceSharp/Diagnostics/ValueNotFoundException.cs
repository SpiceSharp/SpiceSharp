using System;

namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when a value type could not be found.
    /// </summary>
    /// <seealso cref="CircuitException" />
    public class ValueNotFoundException : CircuitException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueNotFoundException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public ValueNotFoundException(Type type)
            : base("A value of type '{0}' could not be found.".FormatString(type.FullName))
        {
        }
    }
}
