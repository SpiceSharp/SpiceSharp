namespace SpiceSharp.Parser
{
    /// <summary>
    /// An @-token (eg. "@M1[gm]")
    /// </summary>
    public class AtToken
    {
        /// <summary>
        /// Gets the name of the At-token (M1 in "@M1[gm]")
        /// </summary>
        public object Name { get; }

        /// <summary>
        /// Gets the parameter of the At-token (gm in "@M1[gm]")
        /// </summary>
        public object Parameter { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameter">Parameter</param>
        public AtToken(object name, object parameter)
        {
            Name = name;
            Parameter = parameter;
        }
    }
}
