using System;

namespace SpiceSharp
{
    /// <summary>
    /// Exception for a bad parameter.
    /// </summary>
    public class BadParameterException : CircuitException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BadParameterException() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameterName">Parameter name</param>
        public BadParameterException(string parameterName)
            : base("Invalid parameter value for '{0}'".FormatString(parameterName))
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameterName">Parameter name</param>
        /// <param name="innerException">Inner exception</param>
        public BadParameterException(string parameterName, Exception innerException)
            : base("Invalid parameter value for '{0}'".FormatString(parameterName), innerException)
        {
        }
    }
}
