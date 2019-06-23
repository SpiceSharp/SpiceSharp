using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="Inductor"/>
    /// </summary>
    public class BiasingBehavior : ExportingBehavior, IBiasingBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Gets the branch equation index.
        /// </summary>
        /// <value>
        /// The branch equation index.
        /// </value>
        public int BranchEq { get; private set; }

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Current")]
        public double GetCurrent(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return state.Solution[BranchEq];
        }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double GetVoltage(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return state.Solution[PosNode] - state.Solution[NegNode];
        }

        /// <summary>
        /// Gets the power dissipated by the inductor.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("p"), ParameterInfo("Power")]
        public double GetPower(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            var v = state.Solution[PosNode] - state.Solution[NegNode];
            return v * state.Solution[BranchEq];
        }

        /// <summary>
        /// Nodes
        /// </summary>
        protected int PosNode { get; private set; }
        protected int NegNode { get; private set; }
        protected MatrixElement<double> PosBranchPtr { get; private set; }
        protected MatrixElement<double> NegBranchPtr { get; private set; }
        protected MatrixElement<double> BranchNegPtr { get; private set; }
        protected MatrixElement<double> BranchPosPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public BiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup the behavior for the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            pins.ThrowIfNot(nameof(pins), 2);
            PosNode = pins[0];
            NegNode = pins[1];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="variables">Variables</param>
        /// <param name="solver">Solver</param>
        public void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            variables.ThrowIfNull(nameof(variables));
            solver.ThrowIfNull(nameof(solver));

            // Create current equation
            BranchEq = variables.Create(Name.Combine("branch"), VariableType.Current).Index;

            // Get matrix pointers
            PosBranchPtr = solver.GetMatrixElement(PosNode, BranchEq);
            NegBranchPtr = solver.GetMatrixElement(NegNode, BranchEq);
            BranchNegPtr = solver.GetMatrixElement(BranchEq, NegNode);
            BranchPosPtr = solver.GetMatrixElement(BranchEq, PosNode);
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public void Load(BaseSimulation simulation)
        {
            PosBranchPtr.Value += 1;
            NegBranchPtr.Value -= 1;
            BranchPosPtr.Value += 1;
            BranchNegPtr.Value -= 1;
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
