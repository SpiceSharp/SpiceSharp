using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// Create a default resistor model
        /// </summary>
        private static ResistorModel DefaultModel = new ResistorModel(null);

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("temp"), SpiceInfo("Instance operating temperature in Kelvin", Interesting = false)]
        public Parameter<double> REStemp { get; } = new Parameter<double>();
        [SpiceName("resistance"), SpiceInfo("Resistance", IsPrincipal = true)]
        public Parameter<double> RESresist { get; } = new Parameter<double>();
        [SpiceName("w"), SpiceInfo("Width", Interesting = false)]
        public Parameter<double> RESwidth { get; } = new Parameter<double>();
        [SpiceName("l"), SpiceInfo("Length", Interesting = false)]
        public Parameter<double> RESlength { get; } = new Parameter<double>();
        [SpiceName("i"), SpiceInfo("Current")]
        public double GetCurrent(Circuit ckt) => (ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode]) * RESconduct;
        [SpiceName("p"), SpiceInfo("Power")]
        public double GetPower(Circuit ckt) => (ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode]) *
            (ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode]) * RESconduct;

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
            ResistorModel rmod = Model as ResistorModel ?? DefaultModel;

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
