using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read DC analysis
    /// </summary>
    public class DCReader : Reader
    {
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (!name.TryReadLiteral("dc"))
                return false;

            DC dc = new DC("DC " + (netlist.Simulations.Count + 1));
            int count = parameters.Count / 4;
            switch (parameters.Count - 4 * count)
            {
                case 0:
                    if (parameters.Count > 0)
                        throw new ParseException(name, "Source name expected");
                    break;
                case 1: throw new ParseException(parameters[count * 4], "Start value expected");
                case 2: throw new ParseException(parameters[count * 4 + 1], "Stop value expected");
                case 3: throw new ParseException(parameters[count * 4 + 2], "Step value expected");
            }

            // Format: .DC SRCNAM VSTART VSTOP VINCR [SRC2 START2 STOP2 INCR2]
            for (int i = 0; i < count; i++)
            {
                DC.Sweep sweep = new DC.Sweep(
                    parameters[count * 4].ReadWord(),
                    parameters[count * 4 + 1].ReadValue(),
                    parameters[count * 4 + 2].ReadValue(),
                    parameters[count * 4 + 3].ReadValue());
                dc.Sweeps.Add(sweep);
            }

            netlist.Simulations.Add(dc);
            return true;
        }
    }
}
