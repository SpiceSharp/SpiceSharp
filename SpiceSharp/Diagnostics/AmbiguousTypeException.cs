using System;

namespace SpiceSharp.General
{
    /// <summary>
    /// Exception for ambiguous types.
    /// </summary>
    /// <seealso cref="CircuitException" />
    public class AmbiguousTypeException : CircuitException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AmbiguousTypeException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public AmbiguousTypeException(Type type)
            : base("Ambiguous type reference for '{0}'".FormatString(type.FullName))
        {
        }
    }
}
