using SpiceSharp.Parser.Subcircuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read subcircuit definitions
    /// </summary>
    public class SubcircuitDefinitionReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SubcircuitDefinitionReader() : base(StatementType.Subcircuit) { }

        /// <summary>
        /// Read subcircuit definitions
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <param name="netlist"></param>
        /// <returns></returns>
        public override bool Read(Statement st, Netlist netlist)
        {
            if (st.Parameters.Count < 2)
                throw new ParseException(st.Name, "Subcircuit name expected", false);

            // Create a new subcircuit definition
            var def = new SubcircuitDefinition(st);
            netlist.Path.AddDefinition(def);
            Generated = def;
            return true;
        }
    }
}
