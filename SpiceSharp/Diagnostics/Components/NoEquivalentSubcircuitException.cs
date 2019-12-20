using System;

namespace SpiceSharp
{
    /// <summary>
    /// An exception thrown when a subcircuit cannot be represented.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class NoEquivalentSubcircuitException : SpiceSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoEquivalentSubcircuitException"/> class.
        /// </summary>
        public NoEquivalentSubcircuitException() 
            : base(Properties.Resources.Subcircuits_NoEquivalent)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoEquivalentSubcircuitException"/> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        public NoEquivalentSubcircuitException(Exception innerException) 
            : base(Properties.Resources.Subcircuits_NoEquivalent, innerException)
        {
        }
    }
}
