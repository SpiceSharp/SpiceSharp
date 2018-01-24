using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;
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
        /// Get voltage across the voltage source
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [SpiceName("v"), SpiceInfo("Voltage accross the supply")]
        public double GetV(State state) => (state.Solution[ISRCposNode] - state.Solution[ISRCnegNode]);
        [SpiceName("p"), SpiceInfo("Power supplied by the source")]
        public double GetP(State state) => (state.Solution[ISRCposNode] - state.Solution[ISRCposNode]) * -Current;
        [SpiceName("c"), SpiceName("i"), SpiceInfo("Current through current source")]
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
        /// Create an export method
        /// </summary>
        /// <param name="property">Parameter name</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(string property)
        {
            // Avoid using reflection for common components
            switch (property)
            {
                case "v": return GetV;
                case "p": return GetP;
                case "i":
                case "c": return (State state) => Current;
                default: return null;
            }
        }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
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
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            var state = sim.State;
            var rstate = state;

            double value = 0.0;
            double time = 0.0;

            // Time domain analysis
            if (state.Domain == State.DomainTypes.Time)
            {
                if (sim is TimeSimulation tsim)
                    time = tsim.Method.Time;

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
