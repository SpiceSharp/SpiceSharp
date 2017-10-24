using SpiceSharp.Circuits;
using SpiceSharp.Behaviours;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// General behaviour for <see cref="Voltagesource"/>
    /// </summary>
    public class VoltagesourceLoadBehaviour : CircuitObjectBehaviourLoad
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
            var voltageSource = ComponentTyped<Voltagesource>();

            var state = ckt.State;
            var rstate = state.Real;
            double time = 0.0;
            double value = 0.0;

            rstate.Matrix[voltageSource.VSRCposNode, voltageSource.VSRCbranch] += 1.0;
            rstate.Matrix[voltageSource.VSRCbranch, voltageSource.VSRCposNode] += 1.0;
            rstate.Matrix[voltageSource.VSRCnegNode, voltageSource.VSRCbranch] -= 1.0;
            rstate.Matrix[voltageSource.VSRCbranch, voltageSource.VSRCnegNode] -= 1.0;

            if (state.Domain == CircuitState.DomainTypes.Time)
            {
                if (ckt.Method != null)
                    time = ckt.Method.Time;

                // Use the waveform if possible
                if (voltageSource.VSRCwaveform != null)
                    value = voltageSource.VSRCwaveform.At(time);
                else
                    value = voltageSource.VSRCdcValue * state.SrcFact;
            }
            else
            {
                value = voltageSource.VSRCdcValue * state.SrcFact;
            }
            rstate.Rhs[voltageSource.VSRCbranch] += value;
        }
    }
}
