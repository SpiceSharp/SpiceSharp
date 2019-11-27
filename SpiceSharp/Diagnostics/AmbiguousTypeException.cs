using System;

namespace SpiceSharp.General
{
    /// <summary>
    /// Exception for ambiguous types.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class AmbiguousTypeException : SpiceSharpException
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
