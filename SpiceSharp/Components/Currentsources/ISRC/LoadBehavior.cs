using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.CurrentsourceBehaviors
{
    /// <summary>
    /// General behavior for a <see cref="CurrentSource"/>
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
        [PropertyName("v"), PropertyInfo("Voltage accross the supply")]
        public double GetV(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return (state.Solution[posNode] - state.Solution[negNode]);
        }
        [PropertyName("p"), PropertyInfo("Power supplied by the source")]
        public double GetP(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return (state.Solution[posNode] - state.Solution[posNode]) * -Current;
        }
        [PropertyName("c"), PropertyName("i"), PropertyInfo("Current through current source")]
        public double Current { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode;

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
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);

            // Give some warnings if no value is given
            if (!bp.DcValue.Given)
            {
                // no DC value - either have a transient value or none
                if (bp.Waveform != null)
                    CircuitWarning.Warning(this, "{0} has no DC value, transient time 0 value used".FormatString(Name));
                else
                    CircuitWarning.Warning(this, "{0} has no value, DC 0 assumed".FormatString(Name));
            }
        }
        
        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            posNode = pins[0];
            negNode = pins[1];
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var state = simulation.State;

            double value = 0.0;
            double time = 0.0;

            // Time domain analysis
            if (state.Domain == State.DomainTypes.Time)
            {
                if (simulation is TimeSimulation tsim)
                    time = tsim.Method.Time;

                // Use the waveform if possible
                if (bp.Waveform != null)
                    value = bp.Waveform.At(time);
                else
                    value = bp.DcValue * state.SrcFact;
            }
            else
            {
                // AC or DC analysis use the DC value
                value = bp.DcValue * state.SrcFact;
            }

            state.Rhs[posNode] += value;
            state.Rhs[negNode] -= value;
            Current = value;
        }
    }
}
