using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.CurrentSwitchBehaviors
{
    /// <summary>
    /// General behavior for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        ModelLoadBehavior modelload;
        VoltagesourceBehaviors.LoadBehavior vsrcload;
        ModelBaseParameters mbp;

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyName("v"), PropertyInfo("Switch voltage")]
        public double GetVoltage(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[posNode] - state.Solution[negNode];
        }
        [PropertyName("i"), PropertyInfo("Switch current")]
        public double GetCurrent(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return (state.Solution[posNode] - state.Solution[negNode]) * Cond;
        }
        [PropertyName("p"), PropertyInfo("Instantaneous power")]
        public double GetPower(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return (state.Solution[posNode] - state.Solution[negNode]) *
            (state.Solution[posNode] - state.Solution[negNode]) * Cond;
        }

        /// <summary>
        /// Gets or sets the old state of the switch
        /// </summary>
        public bool OldState { get; set; }

        /// <summary>
        /// Flag for using the old state or not
        /// </summary>
        public bool UseOldState { get; set; } = false;

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
        int posNode, negNode;
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
            bp = provider.GetParameterSet<BaseParameters>("entity");
            mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            modelload = provider.GetBehavior<ModelLoadBehavior>("model");
            vsrcload = provider.GetBehavior<VoltagesourceBehaviors.LoadBehavior>("control");
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
                throw new Diagnostics.CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            posNode = pins[0];
            negNode = pins[1];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Nodes nodes, Solver<double> solver)
        {
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            // Get extra equations
            ControllingBranch = vsrcload.BranchEq;

            // Get matrix pointers
            PosPosPtr = solver.GetMatrixElement(posNode, posNode);
            PosNegPtr = solver.GetMatrixElement(posNode, negNode);
            NegPosPtr = solver.GetMatrixElement(negNode, posNode);
            NegNegPtr = solver.GetMatrixElement(negNode, negNode);
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

            double g_now;
            double i_ctrl;
            bool previous_state;
            bool current_state = false;
            var state = simulation.RealState;

            // decide the state of the switch
            if (state.Init == RealState.InitializationStates.InitFix || state.Init == RealState.InitializationStates.InitJunction)
            {
                if (bp.ZeroState)
                {
                    // Switch specified "on"
                    CurrentState = true;
                    current_state = true;
                }
                else
                {
                    // Switch specified "off"
                    CurrentState = false;
                    current_state = false;
                }
            }
            else
            {
                // Get the previous state
                if (UseOldState)
                    previous_state = OldState;
                else
                    previous_state = CurrentState;
                i_ctrl = state.Solution[ControllingBranch];

                // Calculate the current state
                if (i_ctrl > (mbp.Threshold + mbp.Hysteresis))
                    current_state = true;
                else if (i_ctrl < (mbp.Threshold - mbp.Hysteresis))
                    current_state = false;
                else
                    current_state = previous_state;

                // Store the current state
                CurrentState = current_state;

                // If the state changed, ensure one more iteration
                if (current_state != previous_state)
                    state.IsConvergent = false;
            }

            // Get the current conduction
            g_now = current_state == true ? modelload.OnConductance : modelload.OffConductance;
            Cond = g_now;

            // Load the Y-matrix
            PosPosPtr.Value += g_now;
            PosNegPtr.Value -= g_now;
            NegPosPtr.Value -= g_now;
            NegNegPtr.Value += g_now;
        }
    }
}
