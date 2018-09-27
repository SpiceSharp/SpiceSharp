using System;

namespace SpiceSharp
{
    /// <summary>
    /// Exception for identifying a bad parameter.
    /// </summary>
    public class BadParameterException : CircuitException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadParameterException"/> class.
        /// </summary>
        public BadParameterException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadParameterException"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        public BadParameterException(string parameterName)
            : base("Invalid parameter value for '{0}'".FormatString(parameterName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadParameterException"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="innerException">The inner exception.</param>
        public BadParameterException(string parameterName, Exception innerException)
            : base("Invalid parameter value for '{0}'".FormatString(parameterName), innerException)
        {
        }
    }
}
