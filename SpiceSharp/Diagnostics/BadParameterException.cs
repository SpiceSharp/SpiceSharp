using System;

namespace SpiceSharp
{
    /// <summary>
    /// Exception for identifying a bad parameter.
    /// </summary>
    public class BadParameterException : SpiceSharpException
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
            : base(Properties.Resources.BadParameterNamed.FormatString(parameterName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadParameterException"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="reason">The reason.</param>
        public BadParameterException(string parameterName, string reason)
            : base(Properties.Resources.BadParameterValue.FormatString(parameterName, reason))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadParameterException"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="reason">The reason.</param>
        public BadParameterException(string parameterName, object value, string reason)
            : base(Properties.Resources.BadParameterValueReason.FormatString(parameterName, value, reason))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadParameterException"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="innerException">The inner exception.</param>
        public BadParameterException(string parameterName, Exception innerException)
            : base(Properties.Resources.BadParameterReason.FormatString(parameterName, innerException.Message), innerException)
        {
        }
    }
}
