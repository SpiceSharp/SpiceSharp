using SpiceSharp.Entities;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="VoltageDelay" />.
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the branch equation row.
        /// </summary>
        public Variable Branch { get; private set; }

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> Elements { get; private set; }

        /// <summary>
        /// Gets the real state.
        /// </summary>
        protected IBiasingSimulationState BiasingState { get; private set; }

        private int _posNode, _negNode, _contPosNode, _contNegNode, _branchEq;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public BiasingBehavior(string name)
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
            BaseParameters = context.Behaviors.Parameters.GetValue<BaseParameters>();
            BiasingState = context.States.GetValue<IBiasingSimulationState>();
            var c = (ComponentBindingContext)context;
            c.Nodes.ThrowIfNot("nodes", 4);
            _posNode = BiasingState.Map[c.Nodes[0]];
            _negNode = BiasingState.Map[c.Nodes[1]];
            _contPosNode = BiasingState.Map[c.Nodes[2]];
            _contNegNode = BiasingState.Map[c.Nodes[3]];
            Branch = context.Variables.Create(Name.Combine("branch"), VariableType.Current);
            _branchEq = BiasingState.Map[Branch];

            Elements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(_posNode, _branchEq),
                new MatrixLocation(_negNode, _branchEq),
                new MatrixLocation(_branchEq, _posNode),
                new MatrixLocation(_branchEq, _negNode),
                new MatrixLocation(_branchEq, _contPosNode),
                new MatrixLocation(_branchEq, _contNegNode)
            });
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            BiasingState = null;
            Elements?.Destroy();
            Elements = null;
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            if (BiasingState.UseDc)
                Elements.Add(1, -1, 1, -1, -1, 1);
            else
                Elements.Add(1, -1, 1, -1);
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
