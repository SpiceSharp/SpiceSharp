using System.Collections.Generic;
using System.Text;
using System.IO;
using SpiceSharp.Parser.Expressions;
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
        /// An event that can be used for the SpiceMemberConvertData event
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="data">The data</param>
        public void OnSpiceConvert(object sender, SpiceMemberConvertData data)
        {
            if (data.Value is string && data.TargetType == typeof(double))
                data.Result = Parse(data.Value as string);
        }
    }
}
