using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharp.Parser.Readers.Exports
{
    /// <summary>
    /// Read an output
    /// </summary>
    public class VoltageReader : Reader
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
            if (!name.TryReadLiteral("v"))
                return false;

            string node, reference = null;
            switch (parameters.Count)
            {
                case 0: throw new ParseException(name, "Node expected", false);
                case 2: reference = parameters[1].ReadIdentifier(); goto case 1;
                case 1: node = parameters[0].ReadIdentifier(); break;
                default: throw new ParseException(name, "Too many nodes specified", false);
            }

            // Add to the exports
            VoltageExport ve = new VoltageExport(node, reference);
            netlist.Exports.Add(ve);
            Generated = ve;
            return true;
        }
    }

    /// <summary>
    /// A voltage export
    /// </summary>
    public class VoltageExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public string Node { get; }

        /// <summary>
        /// The reference node
        /// </summary>
        public string Reference { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="reference">Reference</param>
        public VoltageExport(string node, string reference = null)
        {
            Node = node;
            Reference = reference;
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "voltage";

        /// <summary>
        /// Get the name based on the properties
        /// </summary>
        public override string Name => "v(" + Node + (Reference == null ? "" : ", " + Reference) + ")";

        /// <summary>
        /// Read the voltage and write to the output
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="ckt">Circuit</param>
        public override object Extract(SimulationData data)
        {
            if (data.Circuit.State.Domain == Circuits.CircuitState.DomainTypes.Frequency || data.Circuit.State.Domain == Circuits.CircuitState.DomainTypes.Laplace)
                return data.GetPhasor(Node, Reference);
            else
                return data.GetVoltage(Node, Reference);
        }
    }
}
