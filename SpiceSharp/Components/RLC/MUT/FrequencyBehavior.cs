using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.InductorBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MutualInductanceBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="MutualInductance"/>
    /// </summary>
    public class FrequencyBehavior : TemperatureBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        protected BiasingBehavior Bias1 { get; private set; }
        protected BiasingBehavior Bias2 { get; private set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement<Complex> Branch1Branch2Ptr { get; private set; }
        protected MatrixElement<Complex> Branch2Branch1Ptr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
			base.Setup(simulation, provider);
			provider.ThrowIfNull(nameof(provider));

            // Get behaviors
            Bias1 = provider.GetBehavior<BiasingBehavior>("inductor1");
            Bias2 = provider.GetBehavior<BiasingBehavior>("inductor2");
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        public void InitializeParameters(FrequencySimulation simulation)
        {
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Matrix</param>
        public void GetEquationPointers(Solver<Complex> solver)
        {
			solver.ThrowIfNull(nameof(solver));

            // Get matrix equations
            Branch1Branch2Ptr = solver.GetMatrixElement(Bias1.BranchEq, Bias2.BranchEq);
            Branch2Branch1Ptr = solver.GetMatrixElement(Bias2.BranchEq, Bias1.BranchEq);
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public void Load(FrequencySimulation simulation)
        {
			simulation.ThrowIfNull(nameof(simulation));

            var state = simulation.ComplexState;
            var value = state.Laplace * Factor;

            // Load Y-matrix
            Branch1Branch2Ptr.Value -= value;
            Branch2Branch1Ptr.Value -= value;
        }
    }
}
