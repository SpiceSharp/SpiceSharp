using static SpiceSharp.Parser.SpiceSharpParserConstants;
namespace SpiceSharp.Parser
{
    /// <summary>
    /// An assignment token
    /// </summary>
    public class AssignmentToken : Token
    {

        /// <summary>
        /// The name of the assignment NAME = VALUE
        /// </summary>
        public Token Name;

        /// <summary>
        /// The value of the assignment NAME = VALUE
        /// </summary>
        public Token Value;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="value">The value</param>
        public AssignmentToken(Token name, Token value)
        {
            Name = name;
            Value = value;
            beginColumn = name.beginColumn;
            endColumn = value.endColumn;
            beginLine = name.beginLine;
            endLine = value.endLine;
            kind = TokenConstants.ASSIGNMENT;
            image = name.image + "=" + value.image;
        }

        /// <summary>
        /// Parse the value
        /// </summary>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public double ParseValue(Netlist netlist)
        {
            if (Value.kind == VALUE)
                return netlist.Readers.ParseDouble(Value.image);
            throw new ParseException(Value, "Value expected");
        }
    }
}
