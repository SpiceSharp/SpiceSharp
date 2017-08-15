using System.Collections.Generic;
using System.Text;
using System.IO;
using SpiceSharp.Parser.Expressions;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Parser.Subcircuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Parser
{
    /// <summary>
    /// A class that can parse spice-type expressions
    /// </summary>
    public class SpiceExpression
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private MemoryStream stream;
        private SpiceSharpExpressionParser parser;

        /// <summary>
        /// Get or set the parameters
        /// </summary>
        public Dictionary<string, double> Parameters
        {
            get
            {
                return parser.Parameters;
            }
            set
            {
                parser.Parameters = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SpiceExpression()
        {
            stream = new MemoryStream();
            parser = new SpiceSharpExpressionParser(stream);
        }

        /// <summary>
        /// Parse an expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public double Parse(string expression)
        {
            byte[] b = Encoding.UTF8.GetBytes(expression);
            stream.SetLength(0);
            stream.Write(b, 0, b.Length);
            stream.Position = 0;
            parser.ReInit(stream);
            return parser.ParseExpression();
        }

        /// <summary>
        /// A method that can be used for the event of parsing data
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="data">The data</param>
        public void OnParseExpression(object sender, ExpressionData data)
        {
            data.Output = Parse(data.Input);
        }

        /// <summary>
        /// A method that can be used for the event of changing the current path
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args">The data</param>
        public void OnSubcircuitPathChanged(object sender, SubcircuitPathChangedEventArgs args)
        {
            Parameters = args.Parameters;
        }
    }
}
