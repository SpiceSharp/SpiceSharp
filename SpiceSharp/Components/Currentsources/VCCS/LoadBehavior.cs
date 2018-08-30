using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltageControlledCurrentSourceBehaviors
{
    /// <summary>
    /// General behavior for a <see cref="VoltageControlledCurrentSource"/>
    /// </summary>
    public class LoadBehavior : BaseLoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
        /// </summary>
        private BaseParameters _bp;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode, _contPosourceNode, _contNegateNode;
        protected MatrixElement<double> PosControlPosPtr { get; private set; }
        protected MatrixElement<double> PosControlNegPtr { get; private set; }
        protected MatrixElement<double> NegControlPosPtr { get; private set; }
        protected MatrixElement<double> NegControlNegPtr { get; private set; }

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double GetVoltage(RealState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));
            
            return state.Solution[_posNode] - state.Solution[_negNode];
        }
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Current")]
        public double GetCurrent(RealState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return (state.Solution[_posNode] - state.Solution[_negNode]) * _bp.Coefficient;
        }
        [ParameterName("p"), ParameterInfo("Power")]
        public double GetPower(RealState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            var v = state.Solution[_posNode] - state.Solution[_negNode];
            return v * v * _bp.Coefficient;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create export method
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="propertyName">Property name</param>
        /// <returns></returns>
        public override Func<double> CreateExport(Simulation simulation, string propertyName)
        {
            // Get the state
            var state = simulation?.States.Get<RealState>();
            if (state == null)
                return null;

            // Avoid reflection for common components
            switch (propertyName)
            {
                case "v": return () => GetVoltage(state);
                case "i":
                case "c": return () => GetCurrent(state);
                case "p": return () => GetPower(state);
                default: return null;
            }
        }

        /// <summary>
        /// Setup the behavior
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
        /// Connect behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            _posNode = pins[0];
            _negNode = pins[1];
            _contPosourceNode = pins[2];
            _contNegateNode = pins[3];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="variables">Variables</param>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));
            PosControlPosPtr = solver.GetMatrixElement(_posNode, _contPosourceNode);
            PosControlNegPtr = solver.GetMatrixElement(_posNode, _contNegateNode);
            NegControlPosPtr = solver.GetMatrixElement(_negNode, _contPosourceNode);
            NegControlNegPtr = solver.GetMatrixElement(_negNode, _contNegateNode);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        /// <param name="simulation"></param>
        public override void Unsetup(Simulation simulation)
        {
            // Remove references
            PosControlPosPtr = null;
            PosControlNegPtr = null;
            NegControlPosPtr = null;
            NegControlNegPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            PosControlPosPtr.Value += _bp.Coefficient;
            PosControlNegPtr.Value -= _bp.Coefficient;
            NegControlPosPtr.Value -= _bp.Coefficient;
            NegControlNegPtr.Value += _bp.Coefficient;
        }
    }
}
