﻿using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// Basing behavior for switches.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="Switches.Parameters"/>
    public class Biasing : Behavior, IBiasingBehavior,
        IParameterized<Parameters>
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
        public Parameters Parameters { get; }

        /// <summary>
        /// The model parameters.
        /// </summary>
        protected readonly ModelParameters ModelParameters;

        /// <summary>
        /// Gets or sets the old state of the switch.
        /// </summary>
        /// <value>
        /// The old state of the switch.
        /// </value>
        protected bool PreviousState { get; set; }

        /// <summary>
        /// Flag for using the old state or not.
        /// </summary>
        /// <value>
        /// If <c>true</c>, the old state is used; otherwise <c>false</c>.
        /// </value>
        protected bool UseOldState { get; set; }

        /// <summary>
        /// Gets the current state of the switch.
        /// </summary>
        /// <value>
        /// The current state of the switch.
        /// </value>
        [ParameterName("state"), ParameterInfo("The current state of the switch.")]
        public bool CurrentState { get; protected set; }

        /// <summary>
        /// Gets the currently active conductance.
        /// </summary>
        /// <value>
        /// The current conductance of the switch.
        /// </value>
        public double Conductance { get; private set; }

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="biasing"]/Voltage/*'/>
        [ParameterName("v"), ParameterInfo("Switch voltage")]
        public double Voltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="biasing"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Switch current")]
        public double Current => Voltage * Conductance;

        /// <include file='Components/Common/docs.xml' path='docs/members[@name="biasing"]/Power/*'/>
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
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        /// <param name="controller">The controller.</param>
        public Biasing(string name, ComponentBindingContext context, Controller controller) : base(name)
        {
            context.ThrowIfNull(nameof(context));

            _iteration = context.GetState<IIterationSimulationState>();
            _controller = controller.ThrowIfNull(nameof(controller));
            ModelParameters = context.ModelBehaviors.GetParameterSet<ModelParameters>();
            Parameters = context.GetParameterSet<Parameters>();

            var state = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(state.GetSharedVariable(context.Nodes[0]), state.GetSharedVariable(context.Nodes[1]));
            _elements = new ElementSet<double>(state.Solver, _variables.GetMatrixLocations(state.Map));
        }

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
