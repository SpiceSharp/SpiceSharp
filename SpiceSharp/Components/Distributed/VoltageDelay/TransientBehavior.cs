using SpiceSharp.Algebra;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Distributed;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Time behavior for a <see cref="VoltageDelay"/>.
    /// </summary>
    public class TransientBehavior : BiasingBehavior, ITimeBehavior
    {
        /// <summary>
        /// Gets the vector elements.
        /// </summary>
        /// <value>
        /// The vector elements.
        /// </value>
        protected RealVectorElementSet VectorElements { get; private set; }

        /// <summary>
        /// Gets the delayed signal.
        /// </summary>
        public DelayedSignal Signal { get; private set; }

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
        /// Binds the specified simulation.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            VectorElements = new RealVectorElementSet(BiasingState.Solver, BranchEq);
            Signal = new DelayedSignal(1, BaseParameters.Delay);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            VectorElements?.Destroy();
            VectorElements = null;
        }

        /// <summary>
        /// Calculates the state values from the current DC solution.
        /// </summary>
        /// <remarks>
        /// In this method, the initial value is calculated based on the operating point solution,
        /// and the result is stored in each respective <see cref="SpiceSharp.IntegrationMethods.StateDerivative" /> or <see cref="SpiceSharp.IntegrationMethods.StateHistory" />.
        /// </remarks>
        void ITimeBehavior.InitializeStates()
        {
            var sol = BiasingState.Solution;
            var input = sol[ContPosNode] - sol[ContNegNode];
            Signal.SetProbedValues(input);
        }

        /// <summary>
        /// Perform time-dependent calculations.
        /// </summary>
        void ITimeBehavior.Load()
        {
            var sol = BiasingState.Solution;
            var input = sol[ContPosNode] - sol[ContNegNode];
            Signal.SetProbedValues(input);
            VectorElements.Add(Signal.Values[0]);
        }
    }
}
