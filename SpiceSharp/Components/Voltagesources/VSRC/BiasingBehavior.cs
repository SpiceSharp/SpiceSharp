using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;

namespace SpiceSharp.Components.VoltageSourceBehaviors
{
    /// <summary>
    /// General behavior for <see cref="VoltageSource"/>
    /// </summary>
    public class BiasingBehavior : ExportingBehavior, IBiasingBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        /// <value>
        /// The base parameters.
        /// </value>
        protected CommonBehaviors.IndependentSourceParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("i"), ParameterInfo("Voltage source current")]
        public double GetCurrent(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return state.Solution[BranchEq];
        }

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("p"), ParameterInfo("Instantaneous power")]
        public double GetPower(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return (state.Solution[PosNode] - state.Solution[NegNode]) * -state.Solution[BranchEq];
        }

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        [ParameterName("v"), ParameterInfo("Instantaneous voltage")]
        public double Voltage { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        protected int PosNode { get; private set; }
        protected int NegNode { get; private set; }
        public int BranchEq { get; private set; }
        protected MatrixElement<double> PosBranchPtr { get; private set; }
        protected MatrixElement<double> NegBranchPtr { get; private set; }
        protected MatrixElement<double> BranchPosPtr { get; private set; }
        protected MatrixElement<double> BranchNegPtr { get; private set; }
        protected VectorElement<double> BranchPtr { get; private set; }

        /// <summary>
        /// Constructor
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
            BaseParameters = provider.GetParameterSet<CommonBehaviors.IndependentSourceParameters>();

            // Setup the waveform
            BaseParameters.Waveform?.Setup();

            if (!BaseParameters.DcValue.Given)
            {
                // No DC value: either have a transient value or none
                if (BaseParameters.Waveform != null)
                {
                    CircuitWarning.Warning(this, "{0}: No DC value, transient time 0 value used".FormatString(Name));
                    BaseParameters.DcValue.RawValue = BaseParameters.Waveform.Value;
                }
                else
                {
                    CircuitWarning.Warning(this, "{0}: No value, DC 0 assumed".FormatString(Name));
                }
            }
        }

        /// <summary>
        /// Connect the load behavior
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

            BranchEq = variables.Create(Name.Combine("branch"), VariableType.Current).Index;

            // Get matrix elements
            PosBranchPtr = solver.GetMatrixElement(PosNode, BranchEq);
            BranchPosPtr = solver.GetMatrixElement(BranchEq, PosNode);
            NegBranchPtr = solver.GetMatrixElement(NegNode, BranchEq);
            BranchNegPtr = solver.GetMatrixElement(BranchEq, NegNode);

            // Get rhs elements
            BranchPtr = solver.GetRhsElement(BranchEq);
        }
        
        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public void Load(BaseSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            var state = simulation.RealState;
            double value;

            PosBranchPtr.Value += 1;
            BranchPosPtr.Value += 1;
            NegBranchPtr.Value -= 1;
            BranchNegPtr.Value -= 1;

            if (simulation is TimeSimulation)
            {
                // Use the waveform if possible
                if (BaseParameters.Waveform != null)
                    value = BaseParameters.Waveform.Value;
                else
                    value = BaseParameters.DcValue * state.SourceFactor;
            }
            else
            {
                value = BaseParameters.DcValue * state.SourceFactor;
            }

            Voltage = value;
            BranchPtr.Value += value;
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
