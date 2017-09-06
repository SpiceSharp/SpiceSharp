namespace SpiceSharp.Parameters
{
    public interface IParameterized
    {
        /// <summary>
        /// Set a parameter by name
        /// </summary>
        /// <param name="parameter">The parameter name</param>
        /// <param name="value">The value</param>
        void Set(string parameter, double value);

        /// <summary>
        /// Ask a parameter by name
        /// </summary>
        /// <param name="parameter">The parameter name</param>
        /// <returns></returns>
        double Ask(string parameter);

        /// <summary>
        /// Ask a parameter by name
        /// </summary>
        /// <param name="parameter">The parameter name</param>
        /// <param name="ckt">The circuit</param>
        /// <returns></returns>
        double Ask(string parameter, Circuit ckt);

        /// <summary>
        /// Set a parameter by name
        /// </summary>
        /// <param name="parameter">The parameter name</param>
        /// <param name="value">The value</param>
        void Set(string parameter, string value);

        /// <summary>
        /// Ask a parameter by name
        /// </summary>
        /// <param name="parameter">The parameter name</param>
        /// <returns></returns>
        string AskString(string parameter);
    }
}
