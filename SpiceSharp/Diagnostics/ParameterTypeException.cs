using System;
using SpiceSharp.Parameters;

namespace SpiceSharp.Diagnostics
{
    public class ParameterTypeException : CircuitException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">The Parameterized object</param>
        /// <param name="key">The name of the parameter</param>
        public ParameterTypeException(Parameterized obj, Type expected)
            : base($"{obj.Name}: Type mismatch: {expected.Name} expected")
        { }
    }
}
