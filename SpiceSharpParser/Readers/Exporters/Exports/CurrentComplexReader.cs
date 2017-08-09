using System;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Parser.Readers.Exports
{
    /// <summary>
    /// A class that can read complex Currents
    /// </summary>
    public class CurrentComplexReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CurrentComplexReader() : base(StatementType.Export) { }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(Statement st, Netlist netlist)
        {
            if (st.Name.kind != SpiceSharpParserConstants.WORD)
                return false;

            string source = null;
            switch (st.Parameters.Count)
            {
                case 0: throw new ParseException(st.Name, "Node expected", false);
                case 1: source = st.Parameters[0].ReadIdentifier(); break;
                default: throw new ParseException(st.Name, "Too many st.Parameters");
            }

            Export e = null;
            string type;
            if (st.Name.TryReadWord(out type))
            {
                switch (type)
                {
                    case "ir": e = new CurrentRealExport(source); break;
                    case "ii": e = new CurrentImaginaryExport(source); break;
                    case "im": e = new CurrentMagnitudeExport(source); break;
                    case "ip": e = new CurrentPhaseExport(source); break;
                    case "idb": e = new CurrentDecibelExport(source); break;
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
    /// An export for a real Current
    /// </summary>
    public class CurrentRealExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public CurrentRealExport(string source)
        {
            Source = source;
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "current";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "ir(" + Source + ")";

        /// <summary>
        /// Extract
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override object Extract(SimulationData data)
        {
            var vsrc = data.GetComponent(Source) as Voltagesource;
            switch (data.Circuit.State.Domain)
            {
                case Circuits.CircuitState.DomainTypes.Frequency:
                case Circuits.CircuitState.DomainTypes.Laplace:
                    return vsrc?.GetComplexCurrent(data.Circuit).Real;
                default:
                    return vsrc.GetCurrent(data.Circuit);
            }
        }
    }

    /// <summary>
    /// An export for a imaginary current
    /// </summary>
    public class CurrentImaginaryExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public CurrentImaginaryExport(string source)
        {
            Source = source;
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "current";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "ii(" + Source + ")";

        /// <summary>
        /// Extract
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override object Extract(SimulationData data)
        {
            var vsrc = data.GetComponent(Source) as Voltagesource;
            switch (data.Circuit.State.Domain)
            {
                case Circuits.CircuitState.DomainTypes.Frequency:
                case Circuits.CircuitState.DomainTypes.Laplace:
                    return vsrc?.GetComplexCurrent(data.Circuit).Imaginary;
                default:
                    return 0.0;
            }
        }
    }

    /// <summary>
    /// An export for a current magnitude
    /// </summary>
    public class CurrentMagnitudeExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public CurrentMagnitudeExport(string source)
        {
            Source = source;
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "current";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "im(" + Source + ")";

        /// <summary>
        /// Extract
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override object Extract(SimulationData data)
        {
            var vsrc = data.GetComponent(Source) as Voltagesource;
            switch (data.Circuit.State.Domain)
            {
                case Circuits.CircuitState.DomainTypes.Frequency:
                case Circuits.CircuitState.DomainTypes.Laplace:
                    return vsrc?.GetComplexCurrent(data.Circuit).Magnitude;
                default:
                    return vsrc.GetCurrent(data.Circuit);
            }
        }
    }

    /// <summary>
    /// An export for a current phase
    /// </summary>
    public class CurrentPhaseExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public CurrentPhaseExport(string source)
        {
            Source = source;
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "current";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "ip(" + Source + ")";

        /// <summary>
        /// Extract
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override object Extract(SimulationData data)
        {
            var vsrc = data.GetComponent(Source) as Voltagesource;
            switch (data.Circuit.State.Domain)
            {
                case Circuits.CircuitState.DomainTypes.Frequency:
                case Circuits.CircuitState.DomainTypes.Laplace:
                    Complex c = vsrc?.GetComplexCurrent(data.Circuit) ?? double.NaN;
                    return 180.0 / Math.PI * Math.Atan2(c.Imaginary, c.Real);
                default:
                    return vsrc.GetCurrent(data.Circuit);
            }
        }
    }

    /// <summary>
    /// An export for a current magnitude in decibels
    /// </summary>
    public class CurrentDecibelExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public CurrentDecibelExport(string source)
        {
            Source = source;
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "none";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "idb(" + Source + ")";

        /// <summary>
        /// Extract
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override object Extract(SimulationData data)
        {
            var vsrc = data.GetComponent(Source) as Voltagesource;
            switch (data.Circuit.State.Domain)
            {
                case Circuits.CircuitState.DomainTypes.Frequency:
                case Circuits.CircuitState.DomainTypes.Laplace:
                    var c = vsrc?.GetComplexCurrent(data.Circuit) ?? double.NaN;
                    return 10.0 * Math.Log10(c.Real * c.Real + c.Imaginary + c.Imaginary);
                default:
                    return 20.0 * Math.Log10(vsrc.GetCurrent(data.Circuit));
            }
        }
    }
}
