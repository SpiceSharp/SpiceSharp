using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read a transient analysis
    /// </summary>
    public class TransientReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TransientReader() : base(StatementType.Control) { }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(Statement st, Netlist netlist)
        {
            if (!st.Name.TryReadLiteral("tran"))
                return false;

            Transient tran = new Transient("Transient " + (netlist.Simulations.Count + 1));
            switch (st.Parameters.Count)
            {
                case 0: throw new ParseException(st.Name, "Step expected", false);
                case 1: throw new ParseException(st.Parameters[0], "Maximum time expected", false);
            }

            // Standard st.Parameters
            tran.Set("step", st.Parameters[0].ReadValue());
            tran.Set("stop", st.Parameters[1].ReadValue());

            // Optional st.Parameters
            if (st.Parameters.Count > 2)
                tran.Set("start", st.Parameters[2].ReadValue());
            if (st.Parameters.Count > 3)
                tran.Set("maxstep", st.Parameters[1].ReadValue());

            netlist.Simulations.Add(tran);
            Generated = tran;
            return true;
        }
    }
}
