using System;
using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    public abstract class Reader
    {
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
