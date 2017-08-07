using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read AC analysis
    /// </summary>
    public class ACReader : IReader
    {
        /// <summary>
        /// The last generated object
        /// </summary>
        public object Generated { get; private set; }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (!name.TryReadLiteral("ac"))
                return false;

            AC ac = new AC("AC " + (netlist.Simulations.Count + 1));
            switch (parameters.Count)
            {
                case 0: throw new ParseException(name, "LIN, DEC or OCT expected");
                case 1: throw new ParseException(parameters[0], "Number of points expected");
                case 2: throw new ParseException(parameters[1], "Starting frequency expected");
                case 3: throw new ParseException(parameters[2], "Stopping frequency expected");
            }

            // Standard parameters
            string type = parameters[0].ReadWord();
            switch (type)
            {
                case "lin":
                case "oct":
                case "dec":
                    ac.Set("type", type);
                    break;
                default:
                    throw new ParseException(parameters[0], "LIN, DEC or OCT expected");
            }
            ac.Set("steps", parameters[1].ReadValue());
            ac.Set("start", parameters[2].ReadValue());
            ac.Set("stop", parameters[3].ReadValue());

            Generated = ac;
            netlist.Simulations.Add(ac);
            return true;
        }
    }
}
