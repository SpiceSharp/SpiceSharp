using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Current source behaviour for DC and Transient analysis
    /// </summary>
    public class CurrentsourceLoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(ICircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var isrc = ComponentTyped<Currentsource>();
            if (!isrc.ISRCdcValue.Given)
            {
                // no DC value - either have a transient value or none
                if (isrc.ISRCwaveform != null)
                    CircuitWarning.Warning(this, $"{isrc.Name} has no DC value, transient time 0 value used");
                else
                    CircuitWarning.Warning(this, $"{isrc.Name} has no value, DC 0 assumed");
            }
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var currentSource = ComponentTyped<Currentsource>();

            var state = ckt.State;
            var rstate = state.Real;

            double value = 0.0;
            double time = 0.0;

            // Time domain analysis
            if (state.Domain == CircuitState.DomainTypes.Time)
            {
                if (ckt.Method != null)
                    time = ckt.Method.Time;

                // Use the waveform if possible
                if (currentSource.ISRCwaveform != null)
                    value = currentSource.ISRCwaveform.At(time);
                else
                    value = currentSource.ISRCdcValue * state.SrcFact;
            }
            else
            {
                // AC or DC analysis use the DC value
                value = currentSource.ISRCdcValue * state.SrcFact;
            }

            rstate.Rhs[currentSource.ISRCposNode] += value;
            rstate.Rhs[currentSource.ISRCnegNode] -= value;
            currentSource.Current = value;
        }
    }
}
