using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharp.Parser.Readers.Exports
{
    /// <summary>
    /// A class that can read complex voltages
    /// </summary>
    public class VoltageComplexReader : IReader
    {
        /// <summary>
        /// Get the last generated object
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
            string node, reference = null;
            switch (parameters.Count)
            {
                case 0: throw new ParseException(name, "Node expected", false);
                case 2: reference = parameters[1].ReadIdentifier(); goto case 1;
                case 1: node = parameters[0].ReadIdentifier(); break;
                default: throw new ParseException(name, "Too many parameters");
            }

            Export e = null;
            string type;
            if (name.TryReadWord(out type))
            {
                switch (type)
                {
                    case "vr": e = new VoltageRealExport(node, reference); break;
                    case "vi": e = new VoltageImaginaryExport(node, reference); break;
                    case "vm": e = new VoltageMagnitudeExport(node, reference); break;
                    case "vp": e = new VoltagePhaseExport(node, reference); break;
                    case "vdb": e = new VoltageDecibelExport(node, reference); break;
                    default:
                        return false;
                }

                Generated = e;
                netlist.Exports.Add(e);
                return true;
            }
            else
                return false;
        }
    }

    /// <summary>
    /// An export for a real voltage
    /// </summary>
    public class VoltageRealExport : Export
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
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltageRealExport(string node, string reference = null)
        {
            Node = node;
            Reference = reference;
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "voltage";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "vr(" + Node + (Reference == null ? "" : ", " + Reference) + ")";

        /// <summary>
        /// Extract
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override object Extract(SimulationData data)
        {
            switch (data.Circuit.State.Domain)
            {
                case Circuits.CircuitState.DomainTypes.Frequency:
                case Circuits.CircuitState.DomainTypes.Laplace: return data.GetPhasor(Node, Reference).Real;
                default: return data.GetVoltage(Node, Reference);
            }
        }
    }

    /// <summary>
    /// An export for an imaginary voltage
    /// </summary>
    public class VoltageImaginaryExport : Export
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
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltageImaginaryExport(string node, string reference = null)
        {
            Node = node;
            Reference = reference;
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "voltage";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "vi(" + Node + (Reference == null ? "" : ", " + Reference) + ")";

        /// <summary>
        /// Extract
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override object Extract(SimulationData data)
        {
            switch (data.Circuit.State.Domain)
            {
                case Circuits.CircuitState.DomainTypes.Frequency:
                case Circuits.CircuitState.DomainTypes.Laplace: return data.GetPhasor(Node, Reference).Imaginary;
                default: return 0.0;
            }
        }
    }

    /// <summary>
    /// An export for a voltage magnitude
    /// </summary>
    public class VoltageMagnitudeExport : Export
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
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltageMagnitudeExport(string node, string reference = null)
        {
            Node = node;
            Reference = reference;
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "voltage";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "vm(" + Node + (Reference == null ? "" : ", " + Reference) + ")";

        /// <summary>
        /// Extract
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override object Extract(SimulationData data)
        {
            switch (data.Circuit.State.Domain)
            {
                case Circuits.CircuitState.DomainTypes.Frequency:
                case Circuits.CircuitState.DomainTypes.Laplace: return data.GetPhasor(Node, Reference).Magnitude;
                default: return data.GetVoltage(Node, Reference);
            }
        }
    }

    /// <summary>
    /// An export for a voltage phase
    /// </summary>
    public class VoltagePhaseExport : Export
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
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltagePhaseExport(string node, string reference = null)
        {
            Node = node;
            Reference = reference;
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "degrees";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "vp(" + Node + (Reference == null ? "" : ", " + Reference) + ")";

        /// <summary>
        /// Extract
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override object Extract(SimulationData data)
        {
            switch (data.Circuit.State.Domain)
            {
                case Circuits.CircuitState.DomainTypes.Frequency:
                case Circuits.CircuitState.DomainTypes.Laplace: return data.GetPhase(Node, Reference);
                default: return 0.0;
            }
        }
    }

    /// <summary>
    /// An export for voltage magnitude in decibels
    /// </summary>
    public class VoltageDecibelExport : Export
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
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltageDecibelExport(string node, string reference = null)
        {
            Node = node;
            Reference = reference;
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "none";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "vdb(" + Node + (Reference == null ? "" : ", " + Reference) + ")";

        /// <summary>
        /// Extract
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override object Extract(SimulationData data)
        {
            switch (data.Circuit.State.Domain)
            {
                case Circuits.CircuitState.DomainTypes.Frequency:
                case Circuits.CircuitState.DomainTypes.Laplace: return data.GetDb(Node, Reference);
                default: return 20.0 * Math.Log10(data.GetVoltage(Node, Reference));
            }
        }
    }
}
