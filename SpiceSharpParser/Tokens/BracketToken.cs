namespace SpiceSharp.Parser
{
    /// <summary>
    /// A bracketted token (TOKEN TOKEN ...) or [TOKEN TOKEN ...], ...
    /// </summary>
    public class BracketToken : Token
    {
        /// <summary>
        /// The name of the bracket token NAME(PAR1 PAR2 ...)
        /// </summary>
        public Token Name;

        /// <summary>
        /// The parameters of the bracket token NAME(PAR1 PAR2 ...)
        /// </summary>
        public Token[] Parameters { get; }

        /// <summary>
        /// Get the type of bracket used (default '(')
        /// </summary>
        public char Bracket { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name-token</param>
        /// <param name="bracket">The bracket character (eg. '(' or '[')</param>
        /// <param name="parameters">The parameters specified between brackets</param>
        public BracketToken(Token name, char bracket, Token[] parameters)
        {
            Name = name;
            Bracket = bracket;
            Parameters = parameters;
            beginLine = name.beginLine;
            beginColumn = name.beginColumn;
            endLine = parameters.Length > 0 ? parameters[parameters.Length - 1].endLine : name.endLine;
            endColumn = parameters.Length > 0 ? parameters[parameters.Length - 1].endLine : name.endLine;
            kind = TokenConstants.BRACKET;

            // Create the image
            switch (bracket)
            {
                case '(': image = name.image + "()"; break;
                case '[': image = name.image + "[]"; break;
                case '{': image = name.image + "{}"; break;
                default:
                    throw new ParseException(name, "Invalid bracket", false);
            }
        }
    }
}
