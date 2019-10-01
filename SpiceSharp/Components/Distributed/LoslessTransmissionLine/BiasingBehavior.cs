using SpiceSharp.Circuits;
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
        /// Gets the left-side positive node.
        /// </summary>
        protected int Pos1 { get; private set; }

        /// <summary>
        /// Gets the left-side negative node.
        /// </summary>
        protected int Neg1 { get; private set; }

        /// <summary>
        /// Gets the right-side positive node.
        /// </summary>
        protected int Pos2 { get; private set; }

        /// <summary>
        /// Gets the right-side negative node.
        /// </summary>
        protected int Neg2 { get; private set; }

        /// <summary>
        /// Gets the left-side internal node.
        /// </summary>
        public int Internal1 { get; private set; }

        /// <summary>
        /// Gets the right-side internal node.
        /// </summary>
        public int Internal2 { get; private set; }

        /// <summary>
        /// Gets the left-side branch equation row.
        /// </summary>
        public int BranchEq1 { get; private set; }

        /// <summary>
        /// Gets the right-side branch equation row.
        /// </summary>
        public int BranchEq2 { get; private set; }

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
        protected BiasingSimulationState BiasingState { get; private set; }

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
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            // Get parameters
            BaseParameters = context.Behaviors.Parameters.GetValue<BaseParameters>();

            // Connect
            var c = (ComponentBindingContext)context;
            Pos1 = c.Pins[0];
            Neg1 = c.Pins[1];
            Pos2 = c.Pins[2];
            Neg2 = c.Pins[3];
            var variables = context.Variables;
            Internal1 = variables.Create(Name.Combine("int1"), VariableType.Voltage).Index;
            Internal2 = variables.Create(Name.Combine("int2"), VariableType.Voltage).Index;
            BranchEq1 = variables.Create(Name.Combine("branch1"), VariableType.Current).Index;
            BranchEq2 = variables.Create(Name.Combine("branch2"), VariableType.Current).Index;

            // Get matrix elements
            BiasingState = context.States.GetValue<BiasingSimulationState>();
            Elements = new ElementSet<double>(BiasingState.Solver,
                new MatrixLocation(Pos1, Pos1),
                new MatrixLocation(Pos1, Internal1),
                new MatrixLocation(Internal1, Pos1),
                new MatrixLocation(Internal1, Internal1),
                new MatrixLocation(Internal1, BranchEq1),
                new MatrixLocation(BranchEq1, Internal1),
                new MatrixLocation(Neg1, BranchEq1),
                new MatrixLocation(BranchEq1, Neg1),
                new MatrixLocation(Pos2, Pos2),
                new MatrixLocation(Pos2, Internal2),
                new MatrixLocation(Internal2, Pos2),
                new MatrixLocation(Internal2, Internal2),
                new MatrixLocation(Internal2, BranchEq2),
                new MatrixLocation(BranchEq2, Internal2),
                new MatrixLocation(Neg2, BranchEq2),
                new MatrixLocation(BranchEq2, Neg2),

                // These are only used to calculate the biasing point
                new MatrixLocation(BranchEq1, Pos1),
                new MatrixLocation(BranchEq1, Pos2),
                new MatrixLocation(BranchEq1, Neg2),
                new MatrixLocation(BranchEq2, BranchEq1),
                new MatrixLocation(BranchEq2, BranchEq2));
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
            var y = BaseParameters.Admittance;
            if (BiasingState.UseDc)
            {
                Elements.Add(
                    y, -y, -y, y, 1, 0, -1, -1,
                    y, -y, -y, y, 1, 0, -1, 0,
                    1, -1, 1, 1, 1
                    );
            }
            else
            {
                Elements.Add(
                    y, -y, -y, y, 1, 1, -1, -1,
                    y, -y, -y, y, 1, 1, -1, -1
                    );
            }
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
