namespace SpiceSharp.Diagnostics
{
    public class BadParameterException : CircuitException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="param"></param>
        public BadParameterException(string param)
            : base($"Invalid parameter value for '{param}'")
        {
        }
    }
}
