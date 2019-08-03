using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;

namespace SpiceSharp.Components.VoltageControlledCurrentSourceBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="VoltageControlledCurrentSource" />.
    /// </summary>
    public class BiasingBehavior : ExportingBehavior, IBiasingBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// The positive node.
        /// </summary>
        protected int PosNode { get; private set; }

        /// <summary>
        /// The negative index.
        /// </summary>
        protected int NegNode { get; private set; }

        /// <summary>
        /// The controlling positive node.
        /// </summary>
        protected int ContPosNode { get; private set; }

        /// <summary>
        /// The controlling negative node.
        /// </summary>
        protected int ContNegNode { get; private set; }

        /// <summary>
        /// The (pos, ctrlpos) element.
        /// </summary>
        protected MatrixElement<double> PosControlPosPtr { get; private set; }

        /// <summary>
        /// The (neg, ctrlneg) element.
        /// </summary>
        protected MatrixElement<double> PosControlNegPtr { get; private set; }

        /// <summary>
        /// The (neg, ctrlpos) element.
        /// </summary>
        protected MatrixElement<double> NegControlPosPtr { get; private set; }

        /// <summary>
        /// The (neg, ctrlneg) element.
        /// </summary>
        protected MatrixElement<double> NegControlNegPtr { get; private set; }

        /// <summary>
        /// Get the voltage.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double GetVoltage(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return state.Solution[PosNode] - state.Solution[NegNode];
        }

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Current")]
        public double GetCurrent(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return (state.Solution[ContPosNode] - state.Solution[ContNegNode]) * BaseParameters.Coefficient;
        }

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Power")]
        public double GetPower(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            var v = state.Solution[PosNode] - state.Solution[NegNode];
            var i = (state.Solution[ContPosNode] - state.Solution[ContNegNode]) * BaseParameters.Coefficient;
            return -v * i;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public BiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            provider.ThrowIfNull(nameof(provider));

            // Get parameters
            BaseParameters = provider.GetParameterSet<BaseParameters>();
        }

        /// <summary>
        /// Connect behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            pins.ThrowIfNot(nameof(pins), 4);
            PosNode = pins[0];
            NegNode = pins[1];
            ContPosNode = pins[2];
            ContNegNode = pins[3];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="variables">Variables</param>
        /// <param name="solver">Solver</param>
        public void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            solver.ThrowIfNull(nameof(solver));
            PosControlPosPtr = solver.GetMatrixElement(PosNode, ContPosNode);
            PosControlNegPtr = solver.GetMatrixElement(PosNode, ContNegNode);
            NegControlPosPtr = solver.GetMatrixElement(NegNode, ContPosNode);
            NegControlNegPtr = solver.GetMatrixElement(NegNode, ContNegNode);
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public void Load(BaseSimulation simulation)
        {
            var value = BaseParameters.Coefficient.Value;
            PosControlPosPtr.Value += value;
            PosControlNegPtr.Value -= value;
            NegControlPosPtr.Value -= value;
            NegControlNegPtr.Value += value;
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
