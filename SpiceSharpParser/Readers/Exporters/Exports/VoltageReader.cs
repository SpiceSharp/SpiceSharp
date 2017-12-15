using System;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharp.Parser.Readers.Exports
{
    /// <summary>
    /// Reads a voltage export (V, VR, VI, VDB, VP).
    /// </summary>
    public class VoltageReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public VoltageReader() : base(StatementType.Export)
        {
            Identifier = "v;vr;vi;vdb;vp";
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
            // Get the nodes
            Identifier node, reference = null;
            switch (st.Parameters.Count)
            {
                case 0:
                    throw new ParseException(st.Name, "Node expected", false);
                case 2:
                    if (!ReaderExtension.IsNode(st.Parameters[1]))
                        throw new ParseException(st.Parameters[1], "Node expected");
                    reference = new Identifier(st.Parameters[1].image);
                    goto case 1;
                case 1:
                    if (!ReaderExtension.IsNode(st.Parameters[0]))
                        throw new ParseException(st.Parameters[0], "Node expected");
                    node = new Identifier(st.Parameters[0].image);
                    break;
                default:
                    throw new ParseException(st.Name, "Too many nodes specified", false);
            }

            // Add to the exports
            Export ve = null;
            switch (type)
            {
                case "v": ve = new VoltageExport(node, reference); break;
                case "vr": ve = new VoltageRealExport(node, reference); break;
                case "vi": ve = new VoltageImaginaryExport(node, reference); break;
                case "vdb": ve = new VoltageDecibelExport(node, reference); break;
                case "vp": ve = new VoltagePhaseExport(node, reference); break;
            }

            if (ve != null)
                netlist.Exports.Add(ve);
            Generated = ve;
            return true;
        }
    }

    /// <summary>
    /// Voltage export.
    /// </summary>
    public class VoltageExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Node { get; }

        /// <summary>
        /// The reference node
        /// </summary>
        public Identifier Reference { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="reference">Reference</param>
        public VoltageExport(Identifier node, Identifier reference = null)
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
        public override string Name => "v(" + Node.ToString() + (Reference == null ? "" : ", " + Reference.ToString()) + ")";

        /// <summary>
        /// Read the voltage and write to the output
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="ckt">Circuit</param>
        public override double Extract(SimulationData data)
        {
            if (data.Circuit.State.Domain == Circuits.State.DomainTypes.Frequency || data.Circuit.State.Domain == Circuits.State.DomainTypes.Laplace)
                return data.GetPhasor(Node, Reference).Real;
            else
                return data.GetVoltage(Node, Reference);
        }
    }

    /// <summary>
    /// Real part of a complex voltage export.
    /// </summary>
    public class VoltageRealExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Node { get; }

        /// <summary>
        /// The reference node
        /// </summary>
        public Identifier Reference { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltageRealExport(Identifier node, Identifier reference = null)
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
        public override double Extract(SimulationData data)
        {
            switch (data.Circuit.State.Domain)
            {
                case Circuits.State.DomainTypes.Frequency:
                case Circuits.State.DomainTypes.Laplace:
                    return data.GetPhasor(Node, Reference).Real;
                default:
                    return data.GetVoltage(Node, Reference);
            }
        }
    }

    /// <summary>
    /// Imaginary part of a complex voltage export.
    /// </summary>
    public class VoltageImaginaryExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Node { get; }

        /// <summary>
        /// The reference node
        /// </summary>
        public Identifier Reference { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltageImaginaryExport(Identifier node, Identifier reference = null)
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
        public override double Extract(SimulationData data)
        {
            switch (data.Circuit.State.Domain)
            {
                case State.DomainTypes.Frequency:
                case State.DomainTypes.Laplace:
                    return data.GetPhasor(Node, Reference).Imaginary;
                default:
                    return 0.0;
            }
        }
    }

    /// <summary>
    /// Magnitude of a complex voltage export.
    /// </summary>
    public class VoltageMagnitudeExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Node { get; }

        /// <summary>
        /// The reference node
        /// </summary>
        public Identifier Reference { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltageMagnitudeExport(Identifier node, Identifier reference = null)
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
        public override double Extract(SimulationData data)
        {
            switch (data.Circuit.State.Domain)
            {
                case Circuits.State.DomainTypes.Frequency:
                case Circuits.State.DomainTypes.Laplace:
                    return data.GetPhasor(Node, Reference).Magnitude;
                default:
                    return data.GetVoltage(Node, Reference);
            }
        }
    }

    /// <summary>
    /// Phase of a complex voltage export.
    /// </summary>
    public class VoltagePhaseExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Node { get; }

        /// <summary>
        /// The reference node
        /// </summary>
        public Identifier Reference { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltagePhaseExport(Identifier node, Identifier reference = null)
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
        public override double Extract(SimulationData data)
        {
            switch (data.Circuit.State.Domain)
            {
                case State.DomainTypes.Frequency:
                case State.DomainTypes.Laplace:
                    return data.GetPhase(Node, Reference);
                default:
                    return 0.0;
            }
        }
    }

    /// <summary>
    /// Magnitude in decibels of a complex voltage export.
    /// </summary>
    public class VoltageDecibelExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Node { get; }

        /// <summary>
        /// The reference node
        /// </summary>
        public Identifier Reference { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltageDecibelExport(Identifier node, Identifier reference = null)
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
        public override double Extract(SimulationData data)
        {
            switch (data.Circuit.State.Domain)
            {
                case State.DomainTypes.Frequency:
                case State.DomainTypes.Laplace:
                    return data.GetDb(Node, Reference);
                default:
                    return 20.0 * Math.Log10(data.GetVoltage(Node, Reference));
            }
        }
    }
}
