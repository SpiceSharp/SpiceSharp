namespace SpiceSharp
{
    /// <summary>
    /// The exception that is thrown when a parameter has not been found.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class ParameterNotFoundException : SpiceSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterNotFoundException"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="source">The source.</param>
        public ParameterNotFoundException(string name, object source)
            : base("Could not find a parameter '{0}' for {0}".FormatString(name, source))
        {
        }
    }
}
