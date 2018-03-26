using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CurrentSwitchBehaviors
{
    /// <summary>
    /// General behavior for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class LoadBehavior : BaseLoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private ModelLoadBehavior _modelload;
        private VoltageSourceBehaviors.LoadBehavior _vsrcload;
        private ModelBaseParameters _mbp;

        /// <summary>
        /// Methods
        /// </summary>
        [ParameterName("v"), ParameterInfo("Switch voltage")]
        public double GetVoltage(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[_posNode] - state.Solution[_negNode];
        }
        [ParameterName("i"), ParameterInfo("Switch current")]
        public double GetCurrent(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return (state.Solution[_posNode] - state.Solution[_negNode]) * Cond;
        }
        [ParameterName("p"), ParameterInfo("Instantaneous power")]
        public double GetPower(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return (state.Solution[_posNode] - state.Solution[_negNode]) *
            (state.Solution[_posNode] - state.Solution[_negNode]) * Cond;
        }

        /// <summary>
        /// Gets or sets the old state of the switch
        /// </summary>
        public bool PreviousState { get; set; }

        /// <summary>
        /// Flag for using the old state or not
        /// </summary>
        public bool UseOldState { get; set; }

        /// <summary>
        /// Gets the current state of the switch
        /// </summary>
        public bool CurrentState { get; protected set; }

        /// <summary>
        /// Gets the current conductance
        /// </summary>
        public double Cond { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode;
        public int ControllingBranch { get; private set; }
        protected MatrixElement<double> PosPosPtr { get; private set; }
        protected MatrixElement<double> NegPosPtr { get; private set; }
        protected MatrixElement<double> PosNegPtr { get; private set; }
        protected MatrixElement<double> NegNegPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>("entity");
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // TODO: This should be part of the parameter
            _mbp.Hysteresis.RawValue = Math.Abs(_mbp.Hysteresis.RawValue);

            // Get behaviors
            _modelload = provider.GetBehavior<ModelLoadBehavior>("model");
            _vsrcload = provider.GetBehavior<VoltageSourceBehaviors.LoadBehavior>("control");
        }

        /// <summary>
        /// Connect
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
        /// Gets matrix pointers
        /// </summary>
        /// <param name="variables">Variables</param>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            // Get extra equations
            ControllingBranch = _vsrcload.BranchEq;

            // Get matrix pointers
            PosPosPtr = solver.GetMatrixElement(_posNode, _posNode);
            PosNegPtr = solver.GetMatrixElement(_posNode, _negNode);
            NegPosPtr = solver.GetMatrixElement(_negNode, _posNode);
            NegNegPtr = solver.GetMatrixElement(_negNode, _negNode);
        }
        
        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            PosPosPtr = null;
            PosNegPtr = null;
            NegPosPtr = null;
            NegNegPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            bool currentState;
            var state = simulation.RealState;

            // decide the state of the switch
            if (state.Init == RealState.InitializationStates.InitFix || state.Init == RealState.InitializationStates.InitJunction)
            {
                if (_bp.ZeroState)
                {
                    // Switch specified "on"
                    CurrentState = true;
                    currentState = true;
                }
                else
                {
                    // Switch specified "off"
                    CurrentState = false;
                    currentState = false;
                }
            }
            else
            {
                // Get the previous state
                var previousState = UseOldState ? PreviousState : CurrentState;
                var iCtrl = state.Solution[ControllingBranch];
                if (UseOldState)
                {
                    // Calculate the current state
                    if (iCtrl > _mbp.Threshold + _mbp.Hysteresis)
                        currentState = true;
                    else if (iCtrl < _mbp.Threshold - _mbp.Hysteresis)
                        currentState = false;
                    else
                        currentState = previousState;
                    CurrentState = currentState;
                    UseOldState = false;
                }
                else
                {
                    PreviousState = CurrentState;

                    // Calculate the current state
                    if (iCtrl > _mbp.Threshold + _mbp.Hysteresis)
                    {
                        CurrentState = true;
                        currentState = true;
                    }
                    else if (iCtrl < _mbp.Threshold - _mbp.Hysteresis)
                    {
                        CurrentState = false;
                        currentState = false;
                    }
                    else
                        currentState = PreviousState;

                    // Ensure one more iteration
                    if (currentState != PreviousState)
                        state.IsConvergent = false;
                }


                // Store the current state
                CurrentState = currentState;

                // If the state changed, ensure one more iteration
                if (currentState != previousState)
                    state.IsConvergent = false;
            }

            // Get the current conduction
            var gNow = currentState ? _modelload.OnConductance : _modelload.OffConductance;
            Cond = gNow;

            // Load the Y-matrix
            PosPosPtr.Value += gNow;
            PosNegPtr.Value -= gNow;
            NegPosPtr.Value -= gNow;
            NegNegPtr.Value += gNow;
        }
    }
}
