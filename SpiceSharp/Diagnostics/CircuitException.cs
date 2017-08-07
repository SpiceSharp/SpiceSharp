using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Diagnostics
{
    /// <summary>
    /// A basic exception class for circuits
    /// </summary>
    [Serializable]
    public class CircuitException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="msg"></param>
        public CircuitException(string msg) : base(msg) { }
    }
}
