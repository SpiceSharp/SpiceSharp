using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read a transient analysis
    /// </summary>
    public class TransientReader : IReader
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
            if (!name.TryReadLiteral("tran"))
                return false;

            Transient tran = new Transient("Transient " + (netlist.Simulations.Count + 1));
            switch (parameters.Count)
            {
                case 0: throw new ParseException(name, "Step expected", false);
                case 1: throw new ParseException(parameters[0], "Maximum time expected", false);
            }

            // Standard parameters
            tran.Set("step", parameters[0].ReadValue());
            tran.Set("stop", parameters[1].ReadValue());

            // Optional parameters
            if (parameters.Count > 2)
                tran.Set("start", parameters[2].ReadValue());
            if (parameters.Count > 3)
                tran.Set("maxstep", parameters[1].ReadValue());

            netlist.Simulations.Add(tran);
            Generated = tran;
            return true;
        }
    }
}
