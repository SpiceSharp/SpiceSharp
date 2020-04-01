using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// (DC) biasing behavior for switches.
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior,
        IParameterized<BaseParameters>
    {
        private readonly Controller _controller;
        private readonly IIterationSimulationState _iteration;
        private readonly ElementSet<double> _elements;
        private readonly OnePort<double> _variables;

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public BaseParameters Parameters { get; }

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
        public double Voltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Gets the current through the switch.
        /// </summary>
        /// <returns></returns>
        [ParameterName("i"), ParameterInfo("Switch current")]
        public double Current => Voltage * Conductance;

        /// <summary>
        /// Gets the power dissipated by the switch.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterInfo("Instantaneous power")]
        public double Power
        {
            get
            {
                var v = Voltage;
                return v * v * Conductance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        /// <param name="controller">The controller.</param>
        public BiasingBehavior(string name, ComponentBindingContext context, Controller controller) : base(name)
        {
            context.ThrowIfNull(nameof(context));

            _iteration = context.GetState<IIterationSimulationState>();
            _controller = controller.ThrowIfNull(nameof(controller));
            ModelParameters = context.ModelBehaviors.GetParameterSet<ModelBaseParameters>();
            Parameters = context.GetParameterSet<BaseParameters>();

            var state = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(state.GetSharedVariable(context.Nodes[0]), state.GetSharedVariable(context.Nodes[1]));
            _elements = new ElementSet<double>(state.Solver, _variables.GetMatrixLocations(state.Map));
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            bool currentState;

            // decide the state of the switch
            if (_iteration.Mode == IterationModes.Fix || _iteration.Mode == IterationModes.Junction)
            {
                if (Parameters.ZeroState)
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
                var ctrl = _controller.Value;
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
                        _iteration.IsConvergent = false;
                }

                // Store the current state
                CurrentState = currentState;

                // If the state changed, ensure one more iteration
                if (currentState != PreviousState)
                    _iteration.IsConvergent = false;
            }

            // Get the current conduction
            var gNow = currentState ? ModelParameters.OnConductance : ModelParameters.OffConductance;
            Conductance = gNow;

            // Load the Y-matrix
            _elements.Add(gNow, -gNow, -gNow, gNow);
        }
    }
}
