using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltageControlledCurrentSourceBehaviors2
{
    /// <summary>
    /// General behavior for a <see cref="CurrentSource"/>
    /// </summary>
    public class LoadBehavior : BaseLoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private BaseParameters _bp;

        /// <summary>
        /// Gets voltage across the voltage source
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [ParameterName("v"), ParameterInfo("Voltage accross the supply")]
        public double GetV(RealState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return state.Solution[_posNode] - state.Solution[_negNode];
        }
        [ParameterName("p"), ParameterInfo("Power supplied by the source")]
        public double GetP(RealState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return (state.Solution[_posNode] - state.Solution[_posNode]) * -Current;
        }
        [ParameterName("c"), ParameterName("i"), ParameterInfo("Current through current source")]
        public double Current { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode;
        private VectorElement<double> _posPtr, _negPtr;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create an export method
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="propertyName">Parameter name</param>
        /// <returns></returns>
        public override Func<double> CreateGetter(Simulation simulation, string propertyName)
        {
            // Get the state
            var state = simulation?.States.Get<RealState>();
            if (state == null)
                return null;

            // Avoid using reflection for common components
            switch (propertyName)
            {
                case "v": return () => GetV(state);
                case "p": return () => GetP(state);
                case "i":
                case "c": return () => Current;
                default: return null;
            }
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
            _bp = provider.GetParameterSet<BaseParameters>();
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
        /// Get the matrix elements
        /// </summary>
        /// <param name="variables">Variables</param>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            _posPtr = solver.GetRhsElement(_posNode);
            _negPtr = solver.GetRhsElement(_negNode);
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            double value = _bp.Coefficient.Value();

            _posPtr.Value -= value;
            _negPtr.Value += value;
            Current = value;
        }
    }
}
