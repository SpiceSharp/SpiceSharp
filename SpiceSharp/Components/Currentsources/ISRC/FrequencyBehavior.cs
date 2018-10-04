using System;
using System.Collections.Generic;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CurrentSourceBehaviors
{
    /// <summary>
    /// Behavior of a currentsource in AC analysis
    /// </summary>
    public class FrequencyBehavior : BaseFrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private FrequencyParameters _ap;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode;
        private Complex _ac;
        protected VectorElement<Complex> PosPtr { get; private set; }
        protected VectorElement<Complex> NegPtr { get; private set; }

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [ParameterName("v"), ParameterInfo("Complex voltage")]
        public Complex GetVoltage(ComplexSimulationState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));
            return state.Solution[_posNode] - state.Solution[_negNode];
        }
        [ParameterName("p"), ParameterInfo("Complex power")]
        public Complex GetPower(ComplexSimulationState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            var v = state.Solution[_posNode] - state.Solution[_negNode];
            return -v * Complex.Conjugate(_ac);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Creates a getter for a complex parameter.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        /// A function get return the value of the specified parameter, or <c>null</c> if no parameter was found.
        /// </returns>
        public override Func<Complex> CreateAcExport(Simulation simulation, string propertyName, IEqualityComparer<string> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<string>.Default;
            if (comparer.Equals("c", propertyName))
                return () => _ac;
            return base.CreateAcExport(simulation, propertyName, comparer);
        }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _ap = provider.GetParameterSet<FrequencyParameters>();

            // Calculate the AC vector
            var radians = _ap.AcPhase * Math.PI / 180.0;
            _ac = new Complex(_ap.AcMagnitude * Math.Cos(radians), _ap.AcMagnitude * Math.Sin(radians));
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
            _posNode = pins[0];
            _negNode = pins[1];
        }

        /// <summary>
        /// Get equation pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            PosPtr = solver.GetRhsElement(_posNode);
            NegPtr = solver.GetRhsElement(_negNode);
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            // NOTE: Spice 3f5's documentation is IXXXX POS NEG VALUE but in the code it is IXXXX NEG POS VALUE
            // I solved it by inverting the current when loading the rhs vector
            PosPtr.Value -= _ac;
            NegPtr.Value += _ac;
        }
    }
}
