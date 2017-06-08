using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Parameters;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A class that represents a resistor
    /// </summary>
    public class Resistor : CircuitComponent
    {
        /// <summary>
        /// Parameter table
        /// </summary>
        private static Dictionary<string, ParameterInfo> parameters = new Dictionary<string, ParameterInfo>()
        {
            { "resistance", new ParameterInfo(ParameterAccess.IOPP, typeof(double), "Resistance", "RESresist") },
            { "temp", new ParameterInfo(ParameterAccess.IOPU, typeof(double), "Instance operating temperature (in Kelvin)", "REStemp") },
            { "l", new ParameterInfo(ParameterAccess.IOPU, typeof(double), "Length", "RESlength") },
            { "w", new ParameterInfo(ParameterAccess.IOPU, typeof(double), "Width", "RESwidth") },
            { "i", new ParameterInfo(ParameterAccess.OP, typeof(double), "Current") },
            { "p", new ParameterInfo(ParameterAccess.OP, typeof(double), "Power") },
        };

        /// <summary>
        /// Parameters
        /// </summary>
        public Parameter<double> REStemp { get; } = new Parameter<double>();
        public Parameter<double> RESresist { get; } = new Parameter<double>();
        public Parameter<double> RESwidth { get; } = new Parameter<double>();
        public Parameter<double> RESlength { get; } = new Parameter<double>();

        /// <summary>
        /// Nodes
        /// </summary>
        public int RESposNode { get; private set; }
        public int RESnegNode { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private double RESconduct = 0.0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the resistor</param>
        public Resistor(string name) : base(name, 2)
        {
        }

        /// <summary>
        /// Ask a parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ckt"></param>
        /// <returns></returns>
        public override object Ask(string name, Circuit ckt = null)
        {
            object result = base.Ask(name, ckt);
            if (result != null)
                return result;

            switch (name)
            {
                case "i":
                    return (ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode]) * RESconduct;
                case "p":
                    return (ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode]) * 
                        (ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode]) *
                        RESconduct;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the parameters
        /// </summary>
        public override Dictionary<string, ParameterInfo> Parameters => parameters;

        /// <summary>
        /// Load the resistor
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            state.Matrix[RESposNode, RESposNode] += RESconduct;
            state.Matrix[RESnegNode, RESnegNode] += RESconduct;
            state.Matrix[RESposNode, RESnegNode] -= RESconduct;
            state.Matrix[RESnegNode, RESposNode] -= RESconduct;
        }

        /// <summary>
        /// Setup the resistor
        /// </summary>
        /// <param name="ckt"></param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            RESposNode = nodes[0].Index;
            RESnegNode = nodes[1].Index;
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double factor;
            double difference;
            ResistorModel rmod = Model as ResistorModel;

            // Default Value Processing for Resistor Instance
            if (!REStemp.Given) REStemp.Value = ckt.State.Temperature;
            if (!RESwidth.Given) RESwidth.Value = rmod?.RESdefWidth ?? 0.0;
            if (!RESlength.Given) RESlength.Value = 0;
            if (!RESresist.Given)
            {
                if (rmod.RESsheetRes.Given && (rmod.RESsheetRes != 0) && (RESlength != 0))
                {
                    RESresist.Value = rmod.RESsheetRes * (RESlength - rmod.RESnarrow) / (RESwidth - rmod.RESnarrow);
                }
                else
                {
                    CircuitWarning.Warning(this, string.Format("{0}: resistance=0, set to 1000", Name ?? "NULL"));
                    RESresist.Value = 1000;
                }
            }

            if (rmod != null)
            {
                difference = REStemp - rmod.REStnom;
                factor = 1.0 + (rmod.REStempCoeff1) * difference + (rmod.REStempCoeff2) * difference * difference;
            }
            else
            {
                difference = REStemp - 300.15;
                factor = 1.0;
            }

            RESconduct = 1.0 / (RESresist * factor);
        }
    }
}
