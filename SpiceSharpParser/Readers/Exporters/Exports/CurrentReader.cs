using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharp.Parser.Readers.Exports
{
    /// <summary>
    /// This class can read current exports
    /// </summary>
    public class CurrentReader : Reader
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
            if (!name.TryReadLiteral("i"))
                return false;

            string source;
            switch (parameters.Count)
            {
                case 0: throw new ParseException(name, "Voltage source expected", false);
                case 1: source = parameters[0].ReadIdentifier(); break;
                default: throw new ParseException(name, "Too many nodes specified", false);
            }

            // Add to the exports
            CurrentExport ce = new CurrentExport(source);
            netlist.Exports.Add(ce);
            Generated = ce;
            return true;
        }
    }

    /// <summary>
    /// A voltage export
    /// </summary>
    public class CurrentExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node"></param>
        /// <param name="reference"></param>
        public CurrentExport(string source)
        {
            Source = source;
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "current";

        /// <summary>
        /// Get the name based on the properties
        /// </summary>
        public override string Name => "i(" + Source + ")";

        /// <summary>
        /// Read the voltage and write to the output
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Simulation data</param>
        public override object Extract(SimulationData data)
        {
            if (data.Circuit.State.Domain == Circuits.CircuitState.DomainTypes.Frequency || data.Circuit.State.Domain == Circuits.CircuitState.DomainTypes.Laplace)
            {
                var cc = (Components.Voltagesource)data.GetComponent(Source);
                return cc.GetComplexCurrent(data.Circuit);
            }
            else
                return data.GetParameter<double>(Source, "i");
        }
    }
}
