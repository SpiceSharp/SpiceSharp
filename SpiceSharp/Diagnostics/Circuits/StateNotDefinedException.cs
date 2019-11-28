using System;

namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when a simulation state was not defined.
    /// </summary>
    /// <seealso cref="SpiceSharp.SpiceSharpException" />
    public class StateNotDefinedException : SpiceSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateNotDefinedException"/> class.
        /// </summary>
        public StateNotDefinedException(Type type)
            : base(Properties.Resources.Simulations_StateNotDefined.FormatString(type.FullName))
        {
        }
    }
}
