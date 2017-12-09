using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Current source behaviour for DC and Transient analysis
    /// </summary>
    public class CurrentsourceLoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("waveform"), SpiceInfo("The waveform object for this source")]
        public Waveform ISRCwaveform { get; set; } = null;
        [SpiceName("dc"), SpiceInfo("D.C. source value")]
        public Parameter ISRCdcValue { get; } = new Parameter();

        [SpiceName("v"), SpiceInfo("Voltage accross the supply")]
        public double GetV(Circuit ckt)
        {
            return (ckt.State.Solution[ISRCposNode] - ckt.State.Solution[ISRCnegNode]);
        }
        [SpiceName("p"), SpiceInfo("Power supplied by the source")]
        public double GetP(Circuit ckt)
        {
            return (ckt.State.Solution[ISRCposNode] - ckt.State.Solution[ISRCposNode]) * -ISRCdcValue;
        }
        [SpiceName("c"), SpiceInfo("Current through current source")]
        public double Current { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        private int ISRCposNode, ISRCnegNode;

        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);

            var isrc = component as Currentsource;
            if (!ISRCdcValue.Given)
            {
                // no DC value - either have a transient value or none
                if (ISRCwaveform != null)
                    CircuitWarning.Warning(this, $"{isrc.Name} has no DC value, transient time 0 value used");
                else
                    CircuitWarning.Warning(this, $"{isrc.Name} has no value, DC 0 assumed");
            }

            // Copy nodes
            ISRCposNode = isrc.ISRCposNode;
            ISRCnegNode = isrc.ISRCnegNode;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var rstate = state;

            double value = 0.0;
            double time = 0.0;

            // Time domain analysis
            if (state.Domain == CircuitState.DomainTypes.Time)
            {
                if (ckt.Method != null)
                    time = ckt.Method.Time;

                // Use the waveform if possible
                if (ISRCwaveform != null)
                    value = ISRCwaveform.At(time);
                else
                    value = ISRCdcValue * state.SrcFact;
            }
            else
            {
                // AC or DC analysis use the DC value
                value = ISRCdcValue * state.SrcFact;
            }

            rstate.Rhs[ISRCposNode] += value;
            rstate.Rhs[ISRCnegNode] -= value;
            Current = value;
        }
    }
}
