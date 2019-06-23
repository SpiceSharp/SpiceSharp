using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// (DC) biasing behavior for switches.
    /// </summary>
    /// <seealso cref="SpiceSharp.Simulations.Behaviors.ExportingBehavior" />
    /// <seealso cref="SpiceSharp.Behaviors.IBiasingBehavior" />
    /// <seealso cref="SpiceSharp.Components.IConnectedBehavior" />
    public class BiasingBehavior : ExportingBehavior, IBiasingBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        /// <value>
        /// The base parameters.
        /// </value>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        /// <value>
        /// The model parameters.
        /// </value>
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
        /// <value>
        /// The conductance.
        /// </value>
        public double Conductance { get; private set; }

        /// <summary>
        /// Gets the voltage over the switch.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("v"), ParameterInfo("Switch voltage")]
        public double GetVoltage(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return state.Solution[PosNode] - state.Solution[NegNode];
        }

        /// <summary>
        /// Gets the current through the switch.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("i"), ParameterInfo("Switch current")]
        public double GetCurrent(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return (state.Solution[PosNode] - state.Solution[NegNode]) * Conductance;
        }

        /// <summary>
        /// Gets the power dissipated by the switch.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("p"), ParameterInfo("Instantaneous power")]
        public double GetPower(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return (state.Solution[PosNode] - state.Solution[NegNode]) *
                   (state.Solution[PosNode] - state.Solution[NegNode]) * Conductance;
        }

        /// <summary>
        /// Nodes
        /// </summary>
        protected int PosNode { get; private set; }
        protected int NegNode { get; private set; }
        protected MatrixElement<double> PosPosPtr { get; private set; }
        protected MatrixElement<double> NegPosPtr { get; private set; }
        protected MatrixElement<double> PosNegPtr { get; private set; }
        protected MatrixElement<double> NegNegPtr { get; private set; }

        /// <summary>
        /// Gets the method used for switching.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        protected Controller Method { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="method">The method.</param>
        public BiasingBehavior(string name, Controller method) : base(name)
        {
            Method = method;
        }

        /// <summary>
        /// Connect the behavior in the circuit
        /// </summary>
        /// <param name="pins">Pin indices in order</param>
        /// <exception cref="ArgumentNullException">pins</exception>
        public void Connect(params int[] pins)
        {
            pins.ThrowIfNull(nameof(pins));
            PosNode = pins[0];
            NegNode = pins[1];
            Method.Connect(pins);
        }

        /// <summary>
        /// Setup the behavior for the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The provider.</param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            provider.ThrowIfNull(nameof(provider));

            // Get parameters
            BaseParameters = provider.GetParameterSet<BaseParameters>();
            ModelParameters = provider.GetParameterSet<ModelBaseParameters>("model");

            // Allow the controling method to set up
            Method.Setup(simulation, provider);
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading. Additional
        /// equations can also be allocated here.
        /// </summary>
        /// <param name="variables">The variable set.</param>
        /// <param name="solver">The solver.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            solver.ThrowIfNull(nameof(solver));

            // Get matrix pointers
            PosPosPtr = solver.GetMatrixElement(PosNode, PosNode);
            PosNegPtr = solver.GetMatrixElement(PosNode, NegNode);
            NegPosPtr = solver.GetMatrixElement(NegNode, PosNode);
            NegNegPtr = solver.GetMatrixElement(NegNode, NegNode);
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Load(BaseSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            bool currentState;
            var state = simulation.RealState;

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
        /// <param name="simulation">The base simulation.</param>
        /// <returns>
        /// <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvergent(BaseSimulation simulation) => true;
    }
}
