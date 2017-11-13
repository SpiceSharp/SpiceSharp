using SpiceSharp.Simulations;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads a <see cref="OP"/> analysis.
    /// </summary>
    public class OPReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OPReader() : base(StatementType.Control)
        {
            Identifier = "op";
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="st">Statement</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            OP op = new OP("OP " + (netlist.Simulations.Count + 1));
            netlist.Simulations.Add(op);
            Generated = op;
            return true;
        }
    }
}
