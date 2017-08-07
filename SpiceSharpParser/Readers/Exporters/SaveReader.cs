using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// An exporter that can read .save statements
    /// </summary>
    public class SaveReader : IReader
    {
        /// <summary>
        /// Generated objects
        /// </summary>
        public object Generated { get; private set; } = null;

        /// <summary>
        /// Read
        /// This class will export 
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (!name.TryReadLiteral("save"))
                return false;

            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].TryReadBracket(out BracketToken bt, '?'))
                {
                    if (!(bt.Name is Token))
                        throw new ParseException(bt, "Export type expected");
                    Generated = netlist.Readers.Read("exporter", bt.Name as Token, bt.Parameters, netlist);
                }
            }
            return true;
        }
    }
}
