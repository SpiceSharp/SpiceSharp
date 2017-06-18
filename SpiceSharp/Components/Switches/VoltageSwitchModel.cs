using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class describes a model for a voltage-controlled switch
    /// </summary>
    public class VoltageSwitchModel : CircuitModel
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("ron"), SpiceInfo("Resistance when closed")]
        public Parameter<double> VSWon { get; } = new Parameter<double>();
        [SpiceName("roff"), SpiceInfo("Resistance when off")]
        public Parameter<double> VSWoff { get; } = new Parameter<double>();
        [SpiceName("vt"), SpiceInfo("Threshold voltage")]
        public Parameter<double> VSWthresh { get; } = new Parameter<double>();
        [SpiceName("vh"), SpiceInfo("Hysteresis voltage")]
        public Parameter<double> VSWhyst { get; } = new Parameter<double>();
        [SpiceName("gon"), SpiceInfo("Conductance when closed")]
        public double VSWonConduct { get; private set; }
        [SpiceName("goff"), SpiceInfo("Conductance when closed")]
        public double VSWoffConduct { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the model</param>
        public VoltageSwitchModel(string name) : base(name) { }

        /// <summary>
        /// Setup the model
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            if (!VSWon.Given)
            {
                VSWonConduct = 1.0;
                VSWon.Value = 1.0;
            }
            else
                VSWonConduct = 1.0 / VSWon.Value;

            if (!VSWoff.Given)
            {
                VSWoffConduct = ckt.State.Gmin;
                VSWoff.Value = 1.0 / VSWoffConduct;
            }
            else
                VSWoffConduct = 1.0 / VSWoff.Value;
        }
    }
}
