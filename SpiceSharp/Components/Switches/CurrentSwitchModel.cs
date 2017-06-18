using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a model for a current-controlled switch
    /// </summary>
    public class CurrentSwitchModel : CircuitModel
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("ron"), SpiceInfo("Closed resistance")]
        public Parameter<double> CSWon { get; } = new Parameter<double>();
        [SpiceName("roff"), SpiceInfo("Open resistance")]
        public Parameter<double> CSWoff { get; } = new Parameter<double>();
        [SpiceName("it"), SpiceInfo("Threshold current")]
        public Parameter<double> CSWthresh { get; } = new Parameter<double>();
        [SpiceName("ih"), SpiceInfo("Hysteresis current")]
        public Parameter<double> CSWhyst { get; } = new Parameter<double>();
        [SpiceName("gon"), SpiceInfo("Closed conductance")]
        public double CSWonConduct { get; private set; }
        [SpiceName("goff"), SpiceInfo("Open conductance")]
        public double CSWoffConduct { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the model</param>
        public CurrentSwitchModel(string name) : base(name) { }

        /// <summary>
        /// Setup the model
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            if (!CSWon.Given)
            {
                CSWon.Value = 1.0;
                CSWonConduct = 1.0;
            }
            else
                CSWonConduct = 1.0 / CSWon.Value;

            if (!CSWoff.Given)
            {
                CSWoffConduct = ckt.State.Gmin;
                CSWoff.Value = 1.0 / CSWoffConduct;
            }
            else
                CSWoffConduct = 1.0 / CSWoff.Value;
        }
    }
}
