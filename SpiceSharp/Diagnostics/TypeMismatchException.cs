using System;

namespace SpiceSharp
{
    public class TypeMismatchException : CircuitException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMismatchException"/> class.
        /// </summary>
        /// <param name="expected">The expected type.</param>
        /// <param name="actual">The actual type.</param>
        public TypeMismatchException(Type expected, Type actual)
            : base("Type mismatch: '{0}' expected, but got '{1}'".FormatString(expected?.FullName, actual?.FullName))
        {
        }
    }
}
