using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// (DC) biasing behavior for switches.
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        protected ModelBaseParameters ModelParameters { get; private set; }

        /// <summary>
        /// Gets or sets the old state of the switch
        /// </summary>
        protected bool PreviousState { get; set; }

        /// <summary>
        /// Flag for using the old state or not
        /// </summary>
        protected bool UseOldState { get; set; }

        /// <summary>
        /// Gets the current state of the switch
        /// </summary>
        [ParameterName("state"), ParameterInfo("The current state of the switch.")]
        public bool CurrentState { get; protected set; }

        /// <summary>
        /// Gets the currently active conductance.
        /// </summary>
        public double Conductance { get; private set; }

        /// <summary>
        /// Gets the voltage over the switch.
        /// </summary>
        /// <returns></returns>
        [ParameterName("v"), ParameterInfo("Switch voltage")]
        public double GetVoltage() => _state.ThrowIfNotBound(this).Solution[PosNode] - _state.Solution[NegNode];

        /// <summary>
        /// Gets the current through the switch.
        /// </summary>
        /// <returns></returns>
        [ParameterName("i"), ParameterInfo("Switch current")]
        public double GetCurrent() => (_state.ThrowIfNotBound(this).Solution[PosNode] - _state.Solution[NegNode]) * Conductance;

        /// <summary>
        /// Gets the power dissipated by the switch.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterInfo("Instantaneous power")]
        public double GetPower()
        {
            var v = (_state.ThrowIfNotBound(this).Solution[PosNode] - _state.Solution[NegNode]);
            return v * v * Conductance;
        }

        /// <summary>
        /// Gets the positive node.
        /// </summary>
        protected int PosNode { get; private set; }

        /// <summary>
        /// Gets the negative node.
        /// </summary>
        protected int NegNode { get; private set; }

        /// <summary>
        /// Gets the (positive, positive) element.
        /// </summary>
        protected MatrixElement<double> PosPosPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, positive) element.
        /// </summary>
        protected MatrixElement<double> NegPosPtr { get; private set; }

        /// <summary>
        /// Gets the (positive, negative) element.
        /// </summary>
        protected MatrixElement<double> PosNegPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, negative) element.
        /// </summary>
        protected MatrixElement<double> NegNegPtr { get; private set; }

        /// <summary>
        /// Gets the method used for switching.
        /// </summary>
        protected Controller Method { get; }

        // Cache
        private BaseSimulationState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="controller">The controller.</param>
        public BiasingBehavior(string name, Controller controller) : base(name)
        {
            Method = controller.ThrowIfNull(nameof(controller));
        }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Get parameters
            BaseParameters = context.GetParameterSet<BaseParameters>();
            ModelParameters = context.GetParameterSet<ModelBaseParameters>("model");

            // Allow the controling method to set up
            Method.Bind(simulation, context);

            if (context is ComponentBindingContext cc)
            {
                PosNode = cc.Pins[0];
                NegNode = cc.Pins[1];
            }

            _state = ((BaseSimulation)simulation).RealState;
            var solver = _state.Solver;
            PosPosPtr = solver.GetMatrixElement(PosNode, PosNode);
            PosNegPtr = solver.GetMatrixElement(PosNode, NegNode);
            NegPosPtr = solver.GetMatrixElement(NegNode, PosNode);
            NegNegPtr = solver.GetMatrixElement(NegNode, NegNode);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            _state = null;
            PosPosPtr = null;
            PosNegPtr = null;
            NegPosPtr = null;
            NegNegPtr = null;
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            bool currentState;
            var state = _state.ThrowIfNotBound(this);

            // decide the state of the switch
            if (state.Init == InitializationModes.Fix || state.Init == InitializationModes.Junction)
            {
                if (BaseParameters.ZeroState)
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
                var ctrl = Method.GetValue(state);
                if (UseOldState)
                {
                    // Calculate the current state
                    if (ctrl > ModelParameters.Threshold + ModelParameters.Hysteresis)
                        currentState = true;
                    else if (ctrl < ModelParameters.Threshold - ModelParameters.Hysteresis)
                        currentState = false;
                    else
                        currentState = PreviousState;
                    CurrentState = currentState;
                    UseOldState = false;
                }
                else
                {
                    PreviousState = CurrentState;

                    // Calculate the current state
                    if (ctrl > ModelParameters.Threshold + ModelParameters.Hysteresis)
                    {
                        CurrentState = true;
                        currentState = true;
                    }
                    else if (ctrl < ModelParameters.Threshold - ModelParameters.Hysteresis)
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
                if (currentState != PreviousState)
                    state.IsConvergent = false;
            }

            // Get the current conduction
            var gNow = currentState ? ModelParameters.OnConductance : ModelParameters.OffConductance;
            Conductance = gNow;

            // Load the Y-matrix
            PosPosPtr.Value += gNow;
            PosNegPtr.Value -= gNow;
            NegPosPtr.Value -= gNow;
            NegNegPtr.Value += gNow;
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        bool IBiasingBehavior.IsConvergent() => true;
    }
}
