using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.LosslessTransmissionLineBehaviors
{
    /// <summary>
    /// Load behavior for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the left-side internal node.
        /// </summary>
        /// <value>
        /// The left internal node.
        /// </value>
        public Variable Internal1 { get; private set; }

        /// <summary>
        /// Gets the right-side internal node.
        /// </summary>
        /// <value>
        /// The right internal node.
        /// </value>
        public Variable Internal2 { get; private set; }

        /// <summary>
        /// Gets the left-side branch.
        /// </summary>
        /// <value>
        /// The left branch.
        /// </value>
        public Variable Branch1 { get; private set; }

        /// <summary>
        /// Gets the right-side branch.
        /// </summary>
        /// <value>
        /// The right branch.
        /// </value>
        public Variable Branch2 { get; private set; }

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> Elements { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        protected IBiasingSimulationState BiasingState { get; private set; }

        private int _pos1, _neg1, _pos2, _neg2, _int1, _int2, _br1, _br2;

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

            // Get parameters
            BaseParameters = context.Behaviors.Parameters.GetValue<BaseParameters>();

            // Connect
            BiasingState = context.GetState<IBiasingSimulationState>();

            _pos1 = BiasingState.Map[context.Nodes[0]];
            _neg1 = BiasingState.Map[context.Nodes[1]];
            _pos2 = BiasingState.Map[context.Nodes[2]];
            _neg2 = BiasingState.Map[context.Nodes[3]];
            var variables = context.Variables;
            Internal1 = variables.Create(Name.Combine("int1"), VariableType.Voltage);
            _int1 = BiasingState.Map[Internal1];
            Internal2 = variables.Create(Name.Combine("int2"), VariableType.Voltage);
            _int2 = BiasingState.Map[Internal2];
            Branch1 = variables.Create(Name.Combine("branch1"), VariableType.Current);
            _br1 = BiasingState.Map[Branch1];
            Branch2 = variables.Create(Name.Combine("branch2"), VariableType.Current);
            _br2 = BiasingState.Map[Branch2];

            // Get matrix elements
            Elements = new ElementSet<double>(BiasingState.Solver,
                new MatrixLocation(_pos1, _pos1),
                new MatrixLocation(_pos1, _int1),
                new MatrixLocation(_int1, _pos1),
                new MatrixLocation(_int1, _int1),
                new MatrixLocation(_int1, _br1),
                new MatrixLocation(_br1, _int1),
                new MatrixLocation(_neg1, _br1),
                new MatrixLocation(_br1, _neg1),
                new MatrixLocation(_pos2, _pos2),
                new MatrixLocation(_pos2, _int2),
                new MatrixLocation(_int2, _pos2),
                new MatrixLocation(_int2, _int2),
                new MatrixLocation(_int2, _br2),
                new MatrixLocation(_br2, _int2),
                new MatrixLocation(_neg2, _br2),
                new MatrixLocation(_br2, _neg2),

                // These are only used to calculate the biasing point
                new MatrixLocation(_br1, _pos1),
                new MatrixLocation(_br1, _pos2),
                new MatrixLocation(_br1, _neg2),
                new MatrixLocation(_br2, _br1),
                new MatrixLocation(_br2, _br2));
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            var y = BaseParameters.Admittance;
            Elements.Add(
                y, -y, -y, y, 1, 0, -1, -1,
                y, -y, -y, y, 1, 0, -1, 0,
                1, -1, 1, 1, 1
                );
        }
    }
}
