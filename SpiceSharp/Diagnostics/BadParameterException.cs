using System;

namespace SpiceSharp.Diagnostics
{
    /// <summary>
    /// Exception for a bad parameter.
    /// </summary>
    [Serializable]
    public class BadParameterException : CircuitException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameterName"></param>
        public BadParameterException(string parameterName)
            : base("Invalid parameter value for '{0}'".FormatString(parameterName))
        {
        }
    }
}
