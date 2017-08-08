using System;
using System.Collections.Generic;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This interface can read tokens
    /// </summary>
    public abstract class Reader
    {
        /// <summary>
        /// Get the generated object by the reader
        /// </summary>
        public object Generated { get; protected set; } = null;

        /// <summary>
        /// Read a line
        /// </summary>
        /// <param name="name">The opening name/id</param>
        /// <param name="parameters">The following parameters</param>
        /// <param name="netlist">The resulting netlist</param>
        /// <returns></returns>
        public abstract bool Read(Token name, List<Object> parameters, Netlist netlist);
    }
}
