using System;
using System.Numerics;
using SpiceSharp.Simulations;
using SpiceSharp.Components;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers.Exports
{
    /// <summary>
    /// This class can read current exports
    /// </summary>
    public class CurrentReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CurrentReader() : base(StatementType.Export)
        {
            Identifier = "i;ir;ii;idb;ip";
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
            // Get the source name
            string source;
            switch (st.Parameters.Count)
            {
                case 0:
                    throw new ParseException(st.Name, "Voltage source expected", false);
                case 1:
                    if (!ReaderExtension.IsName(st.Parameters[0]))
                        throw new ParseException(st.Parameters[0], "Component name expected");
                    source = st.Parameters[0].image.ToLower();
                    break;
                default:
                    throw new ParseException(st.Name, "Too many nodes specified", false);
            }

            // Add to the exports
            Export ce = null;
            switch (type)
            {
                case "i": ce = new CurrentExport(source); break;
                case "ir": ce = new CurrentRealExport(source); break;
                case "ii": ce = new CurrentImaginaryExport(source); break;
                case "im": ce = new CurrentMagnitudeExport(source); break;
                case "ip": ce = new CurrentPhaseExport(source); break;
                case "idb": ce = new CurrentDecibelExport(source); break;
            }
            if (ce != null)
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
        public override double Extract(SimulationData data)
        {
            Voltagesource vsrc = (Voltagesource)data.GetObject(Source);
            if (data.Circuit.State.Domain == Circuits.CircuitState.DomainTypes.Frequency || data.Circuit.State.Domain == Circuits.CircuitState.DomainTypes.Laplace)
                return vsrc.GetComplexCurrent(data.Circuit).Real;
            else
                return vsrc.GetCurrent(data.Circuit);
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
        public override double Extract(SimulationData data)
        {
            Voltagesource vsrc = (Voltagesource)data.GetObject(Source);
            switch (data.Circuit.State.Domain)
            {
                case Circuits.CircuitState.DomainTypes.Frequency:
                case Circuits.CircuitState.DomainTypes.Laplace:
                    return vsrc.GetComplexCurrent(data.Circuit).Real;
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
        public override double Extract(SimulationData data)
        {
            Voltagesource vsrc = (Voltagesource)data.GetObject(Source);
            switch (data.Circuit.State.Domain)
            {
                case Circuits.CircuitState.DomainTypes.Frequency:
                case Circuits.CircuitState.DomainTypes.Laplace:
                    return vsrc.GetComplexCurrent(data.Circuit).Imaginary;
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
        public override double Extract(SimulationData data)
        {
            Voltagesource vsrc = (Voltagesource)data.GetObject(Source);
            switch (data.Circuit.State.Domain)
            {
                case Circuits.CircuitState.DomainTypes.Frequency:
                case Circuits.CircuitState.DomainTypes.Laplace:
                    return vsrc.GetComplexCurrent(data.Circuit).Magnitude;
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
        public override double Extract(SimulationData data)
        {
            Voltagesource vsrc = (Voltagesource)data.GetObject(Source);
            switch (data.Circuit.State.Domain)
            {
                case Circuits.CircuitState.DomainTypes.Frequency:
                case Circuits.CircuitState.DomainTypes.Laplace:
                    Complex c = vsrc.GetComplexCurrent(data.Circuit);
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
        public override double Extract(SimulationData data)
        {
            Voltagesource vsrc = (Voltagesource)data.GetObject(Source);
            switch (data.Circuit.State.Domain)
            {
                case Circuits.CircuitState.DomainTypes.Frequency:
                case Circuits.CircuitState.DomainTypes.Laplace:
                    Complex c = vsrc.GetComplexCurrent(data.Circuit);
                    return 10.0 * Math.Log10(c.Real * c.Real + c.Imaginary + c.Imaginary);
                default:
                    return 20.0 * Math.Log10(vsrc.GetCurrent(data.Circuit));
            }
        }
    }
}
