using System;

namespace SpiceSharpGenerator.Diagnostics
{
    /// <summary>
    /// An exception that is thrown if behaviors are cyclically dependent.
    /// </summary>
    public class CyclicDependencyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CyclicDependencyException"/> class.
        /// </summary>
        public CyclicDependencyException()
            : base("Cyclic dependency")
        {
        }
    }
}
