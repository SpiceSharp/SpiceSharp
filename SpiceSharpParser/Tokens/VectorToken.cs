namespace SpiceSharp.Parser
{
    /// <summary>
    /// Vector type tokens TOKEN, TOKEN, ... (eg. OUT,GND)
    /// </summary>
    public class VectorToken : Token
    {
        /// <summary>
        /// The tokens
        /// </summary>
        public Token[] Tokens { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vector"></param>
        public VectorToken(Token[] vector)
        {
            Tokens = vector;
            beginLine = vector[0].beginLine;
            beginColumn = vector[0].beginColumn;
            endLine = vector[vector.Length - 1].beginLine;
            endColumn = vector[vector.Length - 1].endColumn;
            kind = TokenConstants.VECTOR;
        }
    }
}
