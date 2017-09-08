using SpiceSharp.Simulations;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads a <see cref="Transient"/> analysis (time-domain analysis).
    /// </summary>
    public class TransientReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TransientReader() : base(StatementType.Control)
        {
            Identifier = "tran";
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            Transient tran = new Transient("Transient " + (netlist.Simulations.Count + 1));
            switch (st.Parameters.Count)
            {
                case 0: throw new ParseException(st.Name, "Step expected", false);
                case 1: throw new ParseException(st.Parameters[0], "Maximum time expected", false);
            }

            // Standard st.Parameters
            tran.Step = netlist.ParseDouble(st.Parameters[0]);
            tran.FinalTime = netlist.ParseDouble(st.Parameters[1]);

            // Optional st.Parameters
            if (st.Parameters.Count > 2)
                tran.InitTime = netlist.ParseDouble(st.Parameters[2]);
            if (st.Parameters.Count > 3)
                tran.MaxStep = netlist.ParseDouble(st.Parameters[3]);

            netlist.Simulations.Add(tran);
            Generated = tran;
            return true;
        }
    }
}
