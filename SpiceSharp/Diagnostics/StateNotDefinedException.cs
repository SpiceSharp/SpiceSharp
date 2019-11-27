using System;

namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when a simulation state was not defined.
    /// </summary>
    /// <seealso cref="SpiceSharp.CircuitException" />
    public class StateNotDefinedException : CircuitException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateNotDefinedException"/> class.
        /// </summary>
        public StateNotDefinedException(Type type)
            : base("A simulation state of type {0} was not defined".FormatString(type.FullName))
        {
        }
    }
}
