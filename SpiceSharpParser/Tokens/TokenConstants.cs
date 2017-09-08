namespace SpiceSharp.Parser
{
    /// <summary>
    /// Extension to <see cref="SpiceSharpParserConstants"/> with our own custom tokens.
    /// </summary>
    public class TokenConstants : SpiceSharpParserConstants
    {
        /// <summary>
        /// Our own constants
        /// </summary>
        public const int VECTOR = -1;
        public const int ASSIGNMENT = -2;
        public const int BRACKET = -3;
        public const int AT = -4;
        public const int BODY = -5;
    }
}
