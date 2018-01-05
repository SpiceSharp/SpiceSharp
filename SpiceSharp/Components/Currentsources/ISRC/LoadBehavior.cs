using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components;
using SpiceSharp.Attributes;
using System;
using SpiceSharp.Components.ISRC;

namespace SpiceSharp.Behaviors.ISRC
{
    /// <summary>
    /// Current source behavior for DC and Transient analysis
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;

        /// <summary>
        /// Methods
        /// </summary>
        /// <param name="ckt"></param>
        /// <returns></returns>
        [SpiceName("v"), SpiceInfo("Voltage accross the supply")]
        public double GetV(Circuit ckt)
        {
            return (ckt.State.Solution[ISRCposNode] - ckt.State.Solution[ISRCnegNode]);
        }
        [SpiceName("p"), SpiceInfo("Power supplied by the source")]
        public double GetP(Circuit ckt)
        {
            return (ckt.State.Solution[ISRCposNode] - ckt.State.Solution[ISRCposNode]) * -bp.ISRCdcValue;
        }
        [SpiceName("c"), SpiceInfo("Current through current source")]
        public double Current { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int ISRCposNode, ISRCnegNode;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }
        
        /// <summary>
        /// Create getter
        /// </summary>
        /// <param name="state">State</param>
        /// <param name="parameter">Parameter name</param>
        /// <returns></returns>
        public override Func<double> CreateGetter(State state, string parameter)
        {
            switch (parameter)
            {
                case "v": return () => state.Solution[ISRCposNode] - state.Solution[ISRCnegNode];
                case "p": return () => (state.Solution[ISRCposNode] - state.Solution[ISRCnegNode]) * -Current;
                case "c": return () => Current;
                default: return null;
            }
        }

        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();

            // Give some warnings if no value is given
            if (!bp.ISRCdcValue.Given)
            {
                // no DC value - either have a transient value or none
                if (bp.ISRCwaveform != null)
                    CircuitWarning.Warning(this, $"{Name} has no DC value, transient time 0 value used");
                else
                    CircuitWarning.Warning(this, $"{Name} has no value, DC 0 assumed");
            }
        }
        
        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            ISRCposNode = pins[0];
            ISRCnegNode = pins[1];
        }
        
        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var rstate = state;

            double value = 0.0;
            double time = 0.0;

            // Time domain analysis
            if (state.Domain == State.DomainTypes.Time)
            {
                if (ckt.Method != null)
                    time = ckt.Method.Time;

                // Use the waveform if possible
                if (bp.ISRCwaveform != null)
                    value = bp.ISRCwaveform.At(time);
                else
                    value = bp.ISRCdcValue * state.SrcFact;
            }
            else
            {
                // AC or DC analysis use the DC value
                value = bp.ISRCdcValue * state.SrcFact;
            }

            rstate.Rhs[ISRCposNode] += value;
            rstate.Rhs[ISRCnegNode] -= value;
            Current = value;
        }
    }
}
