using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="VoltageDelay" />.
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior, IBranchedBehavior<double>,
        IParameterized<BaseParameters>
    {
        private readonly int _posNode, _negNode, _contPosNode, _contNegNode, _branchEq;

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public BaseParameters Parameters { get; }

        /// <summary>
        /// Gets the branch equation row.
        /// </summary>
        public IVariable<double> Branch { get; }

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> BiasingElements { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, ComponentBindingContext context)
            : base(name)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(4);

            Parameters = context.GetParameterSet<BaseParameters>();
            var state = context.GetState<IBiasingSimulationState>();
            _posNode = state.Map[state.MapNode(context.Nodes[0])];
            _negNode = state.Map[state.MapNode(context.Nodes[1])];
            _contPosNode = state.Map[state.MapNode(context.Nodes[2])];
            _contNegNode = state.Map[state.MapNode(context.Nodes[3])];
            Branch = state.Create(Name.Combine("branch"), Units.Ampere);
            _branchEq = state.Map[Branch];

            BiasingElements = new ElementSet<double>(state.Solver, new[] {
                new MatrixLocation(_posNode, _branchEq),
                new MatrixLocation(_negNode, _branchEq),
                new MatrixLocation(_branchEq, _posNode),
                new MatrixLocation(_branchEq, _negNode),
                new MatrixLocation(_branchEq, _contPosNode),
                new MatrixLocation(_branchEq, _contNegNode)
            });
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            BiasingElements.Add(1, -1, 1, -1, -1, 1);
        }
    }
}
