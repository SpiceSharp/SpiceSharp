namespace SpiceSharp.Parser
{
    /// <summary>
    /// An @-token: @TOKEN[TOKEN] (eg. "@M1[gm]")
    /// </summary>
    public class AtToken : Token
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
        public AtToken(Token name, Token parameter)
        {
            Name = name;
            Parameter = parameter;
            beginLine = name.beginLine;
            beginColumn = name.beginColumn;
            endLine = parameter.endLine;
            endColumn = parameter.endColumn;
            kind = TokenConstants.AT;
            image = name.image + "[" + parameter.image + "]";
        }
    }
}
