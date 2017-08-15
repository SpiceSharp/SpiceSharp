using SpiceSharp.Parser.Readers;

namespace SpiceSharp.Parser
{
    /// <summary>
    /// This token represents the body of a subcircuit definition
    /// </summary>
    public class StatementsToken : Token
    {
        /// <summary>
        /// A body of multiple statements
        /// </summary>
        public Statement[] Body { get; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="body"></param>
        public StatementsToken(Statement[] body)
        {
            Body = body;
            beginColumn = body[0].Name.beginColumn;
            beginLine = body[0].Name.beginLine;
            Statement last = body[body.Length - 1];
            Token lt;
            if (last.Parameters.Count > 0)
                lt = last.Parameters[last.Parameters.Count - 1];
            else
                lt = last.Name;
            endColumn = lt.endColumn;
            endLine = lt.endLine;
            image = "Body (" + body.Length + " st)";
            kind = TokenConstants.BODY;
        }
    }
}
