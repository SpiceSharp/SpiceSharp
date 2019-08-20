using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Distributed;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.LosslessTransmissionLineBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    public class TransientBehavior : BiasingBehavior, ITimeBehavior
    {
        /// <summary>
        /// Gets the delayed signals.
        /// </summary>
        public DelayedSignal Signals { get; private set; }

        /// <summary>
        /// Gets the left branch RHS element.
        /// </summary>
        protected VectorElement<double> Ibr1Ptr { get; private set; }

        /// <summary>
        /// Gets the right branch RHS element.
        /// </summary>
        protected VectorElement<double> Ibr2Ptr { get; private set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public TransientBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            var solver = ((BaseSimulation)simulation).RealState.Solver;
            Ibr1Ptr = solver.GetRhsElement(BranchEq1);
            Ibr2Ptr = solver.GetRhsElement(BranchEq2);

            Signals = new DelayedSignal(2, BaseParameters.Delay);
        }

        /// <summary>
        /// Initialize the states.
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            var sol = State.ThrowIfNotBound(this).Solution;

            // Calculate the inputs
            var input1 = sol[Pos2] - sol[Neg2] + BaseParameters.Impedance * sol[BranchEq2];
            var input2 = sol[Pos1] - sol[Neg1] + BaseParameters.Impedance * sol[BranchEq1];
            Signals.SetProbedValues(input1, input2);
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void ITimeBehavior.Load()
        {
            var sol = State.Solution;

            // Calculate inputs
            var input1 = sol[Pos2] - sol[Neg2] + BaseParameters.Impedance * sol[BranchEq2];
            var input2 = sol[Pos1] - sol[Neg1] + BaseParameters.Impedance * sol[BranchEq1];
            Signals.SetProbedValues(input1, input2);

            // Update the branch equations
            Ibr1Ptr.Value += Signals.Values[0];
            Ibr2Ptr.Value += Signals.Values[1];
        }
    }
}
