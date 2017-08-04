using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Parser
{
    /// <summary>
    /// This class represents a bracketted token
    /// </summary>
    public class BracketToken
    {
        /// <summary>
        /// The name of the bracket token NAME(PAR1 PAR2 ...)
        /// </summary>
        public object Name;

        /// <summary>
        /// The parameters of the bracket token NAME(PAR1 PAR2 ...)
        /// </summary>
        public List<object> Parameters { get; } = new List<object>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name-token</param>
        public BracketToken(object name)
        {
            Name = name;
        }
    }
}
