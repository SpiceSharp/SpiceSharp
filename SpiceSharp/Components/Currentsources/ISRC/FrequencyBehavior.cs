using System;
using System.Numerics;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentsourceBehaviors
{
    /// <summary>
    /// Behavior of a currentsource in AC analysis
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        FrequencyParameters ap;

        /// <summary>
        /// Nodes
        /// </summary>
        int ISRCposNode, ISRCnegNode;
        Complex ISRCac;

        /// <summary>
        /// Properties
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [PropertyName("v"), PropertyInfo("Complex voltage")]
        public Complex GetVoltage(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return new Complex(
                state.Solution[ISRCposNode] - state.Solution[ISRCnegNode],
                state.iSolution[ISRCposNode] - state.iSolution[ISRCnegNode]
                );
        }
        [PropertyName("p"), PropertyInfo("Complex power")]
        public Complex GetPower(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            Complex v = new Complex(
                state.Solution[ISRCposNode] - state.Solution[ISRCnegNode],
                state.iSolution[ISRCposNode] - state.iSolution[ISRCnegNode]
                );
            return -v * Complex.Conjugate(ISRCac);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create delegate for a property
        /// </summary>
        /// <param name="property">Property name</param>
        /// <returns></returns>
        public override Func<State, Complex> CreateAcExport(string property)
        {
            switch (property)
            {
                case "i":
                case "c": return (State state) => ISRCac;
                default: return base.CreateAcExport(property);
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
            ap = provider.GetParameterSet<FrequencyParameters>(0);

            // Calculate the AC vector
            double radians = ap.ISRCacPhase * Math.PI / 180.0;
            ISRCac = new Complex(ap.ISRCacMag * Math.Cos(radians), ap.ISRCacMag * Math.Sin(radians));
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
                throw new Diagnostics.CircuitException($"Pin count mismatch: 2 pins expected, {pins.Length} given");
            ISRCposNode = pins[0];
            ISRCnegNode = pins[1];
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            var state = sim?.State ?? throw new NullReferenceException();
            state.Rhs[ISRCposNode] += ISRCac.Real;
            state.iRhs[ISRCposNode] += ISRCac.Imaginary;
            state.Rhs[ISRCnegNode] -= ISRCac.Real;
            state.iRhs[ISRCnegNode] -= ISRCac.Imaginary;
        }
    }
}
