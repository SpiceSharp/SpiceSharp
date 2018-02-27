using System;
using System.Numerics;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.NewSparse;

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
        int posNode, negNode;
        Complex ac;
        protected VectorElement<Complex> PosPtr { get; private set; }
        protected VectorElement<Complex> NegPtr { get; private set; }

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [PropertyName("v"), PropertyInfo("Complex voltage")]
        public Complex GetVoltage(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));
            return state.Solution[posNode] - state.Solution[negNode];
        }
        [PropertyName("p"), PropertyInfo("Complex power")]
        public Complex GetPower(ComplexState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            Complex v = state.Solution[posNode] - state.Solution[negNode];
            return -v * Complex.Conjugate(ac);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create delegate for a property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns></returns>
        public override Func<ComplexState, Complex> CreateACExport(string propertyName)
        {
            switch (propertyName)
            {
                case "i":
                case "c": return (ComplexState state) => ac;
                default: return base.CreateACExport(propertyName);
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
            ap = provider.GetParameterSet<FrequencyParameters>("entity");

            // Calculate the AC vector
            double radians = ap.ACPhase * Math.PI / 180.0;
            ac = new Complex(ap.ACMagnitude * Math.Cos(radians), ap.ACMagnitude * Math.Sin(radians));
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
                throw new Diagnostics.CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            posNode = pins[0];
            negNode = pins[1];
        }

        /// <summary>
        /// Get equation pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
            PosPtr = solver.GetRhsElement(posNode);
            NegPtr = solver.GetRhsElement(negNode);
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.ComplexState;
            PosPtr.Value += ac;
            NegPtr.Value -= ac;
        }
    }
}
