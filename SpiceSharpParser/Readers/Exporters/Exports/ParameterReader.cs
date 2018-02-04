using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharp.Parameters;

namespace SpiceSharp.Parser.Readers.Exports
{
    /// <summary>
    /// Reads device parameters (eg. &#64;M1[gm]).
    /// </summary>
    public class ParameterReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ParameterReader() : base(StatementType.Export)
        {
            Identifier = null;
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
            // We don't have an identifier, so we need to check here
            if (st.Name.kind != REFERENCE)
                return false;
            if (st.Parameters.Count != 1 || (st.Parameters[0].kind != WORD && st.Parameters[0].kind != IDENTIFIER))
                return false;

            Identifier component = new Identifier(st.Name.image.Substring(1));
            string parameter = st.Parameters[0].image.ToLower();

            var pe = new ParameterExport(component, parameter);
            netlist.Exports.Add(pe);
            Generated = pe;
            return true;
        }
    }

    /// <summary>
    /// A class that can export a device parameter
    /// </summary>
    public class ParameterExport : Export
    {
        /// <summary>
        /// The component
        /// </summary>
        public Identifier Component { get; }

        /// <summary>
        /// The parameter name
        /// </summary>
        public string Parameter { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="component">The component name</param>
        /// <param name="parameter">The parameter name</param>
        public ParameterExport(Identifier component, string parameter)
        {
            Component = component;
            Parameter = parameter;
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "unknown";

        /// <summary>
        /// The display name of the export
        /// </summary>
        public override string Name => "@" + Component.ToString() + "[" + Parameter.ToString() + "]";

        /// <summary>
        /// Extract the data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override double Extract(SimulationData data)
        {
            // TODO: Needs to be redone
            return double.NaN;
        }
    }
}
