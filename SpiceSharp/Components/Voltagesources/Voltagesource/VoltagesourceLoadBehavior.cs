using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for <see cref="Voltagesource"/>
    /// </summary>
    public class VoltagesourceLoadBehavior : CircuitObjectBehaviorLoad
    {
        public override void Setup(ICircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var vsrc = ComponentTyped<Voltagesource>();

            // Calculate the voltage source's complex value
            if (!vsrc.VSRCdcValue.Given)
            {
                // No DC value: either have a transient value or none
                if (vsrc.VSRCwaveform != null)
                    CircuitWarning.Warning(this, $"{vsrc.Name}: No DC value, transient time 0 value used");
                else
                    CircuitWarning.Warning(this, $"{vsrc.Name}: No value, DC 0 assumed");
            }
        }

        /// <summary>
        /// Execute DC or Transient behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var vsrc = ComponentTyped<Voltagesource>();

            var state = ckt.State;
            var rstate = state;
            double time = 0.0;
            double value = 0.0;

            vsrc.VSRCposIbrptr.Value.Real += 1.0;
            vsrc.VSRCibrPosptr.Value.Real += 1.0;
            vsrc.VSRCnegIbrptr.Value.Real -= 1.0;
            vsrc.VSRCibrNegptr.Value.Real -= 1.0;

            if (state.Domain == CircuitState.DomainTypes.Time)
            {
                if (ckt.Method != null)
                    time = ckt.Method.Time;

                // Use the waveform if possible
                if (vsrc.VSRCwaveform != null)
                    value = vsrc.VSRCwaveform.At(time);
                else
                    value = vsrc.VSRCdcValue * state.SrcFact;
            }
            else
            {
                value = vsrc.VSRCdcValue * state.SrcFact;
            }
            rstate.Rhs[vsrc.VSRCbranch] += value;
        }
    }
}
