using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="VoltageDelay" />.
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior, IBranchedBehavior,
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
        public Variable Branch { get; }

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
            _posNode = state.Map[context.Nodes[0]];
            _negNode = state.Map[context.Nodes[1]];
            _contPosNode = state.Map[context.Nodes[2]];
            _contNegNode = state.Map[context.Nodes[3]];
            Branch = context.Variables.Create(Name.Combine("branch"), VariableType.Current);
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
