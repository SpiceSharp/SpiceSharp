using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

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
        /// Gets the left (positive, positive) element.
        /// </summary>
        protected MatrixElement<double> Pos1Pos1Ptr { get; private set; }

        /// <summary>
        /// Gets the left (positive, internal) element.
        /// </summary>
        protected MatrixElement<double> Pos1Int1Ptr { get; private set; }

        /// <summary>
        /// Gets the left (internal, positive) element.
        /// </summary>
        protected MatrixElement<double> Int1Pos1Ptr { get; private set; }

        /// <summary>
        /// Gets the left (internal, internal) element.
        /// </summary>
        protected MatrixElement<double> Int1Int1Ptr { get; private set; }

        /// <summary>
        /// Gets the left (internal, branch) element.
        /// </summary>
        protected MatrixElement<double> Int1Ibr1Ptr { get; private set; }

        /// <summary>
        /// Gets the left (branch, internal) element.
        /// </summary>
        protected MatrixElement<double> Ibr1Int1Ptr { get; private set; }

        /// <summary>
        /// Gets the left (negative, branch) element.
        /// </summary>
        protected MatrixElement<double> Neg1Ibr1Ptr { get; private set; }

        /// <summary>
        /// Gets the left (branch, negative) element.
        /// </summary>
        protected MatrixElement<double> Ibr1Neg1Ptr { get; private set; }
        
        /// <summary>
        /// Gets the right (positive, positive) element.
        /// </summary>
        protected MatrixElement<double> Pos2Pos2Ptr { get; private set; }

        /// <summary>
        /// Gets the right (positive, internal) element.
        /// </summary>
        protected MatrixElement<double> Pos2Int2Ptr { get; private set; }

        /// <summary>
        /// Gets the right (internal, positive) element.
        /// </summary>
        protected MatrixElement<double> Int2Pos2Ptr { get; private set; }

        /// <summary>
        /// Gets the right (internal, internal) element.
        /// </summary>
        protected MatrixElement<double> Int2Int2Ptr { get; private set; }

        /// <summary>
        /// Gets the right (internal, branch) element.
        /// </summary>
        protected MatrixElement<double> Int2Ibr2Ptr { get; private set; }

        /// <summary>
        /// Gets the right (branch, internal) element.
        /// </summary>
        protected MatrixElement<double> Ibr2Int2Ptr { get; private set; }

        /// <summary>
        /// Gets the right (negative, branch) element.
        /// </summary>
        protected MatrixElement<double> Neg2Ibr2Ptr { get; private set; }

        /// <summary>
        /// Gets the right (branch, negative) element.
        /// </summary>
        protected MatrixElement<double> Ibr2Neg2Ptr { get; private set; }

        /// <summary>
        /// Gets the left (branch, positive) element.
        /// </summary>
        protected MatrixElement<double> Ibr1Pos1Ptr { get; private set; }

        /// <summary>
        /// Gets the (left branch, right positive) element.
        /// </summary>
        protected MatrixElement<double> Ibr1Pos2Ptr { get; private set; }

        /// <summary>
        /// Gets the (left branch, right negative) element.
        /// </summary>
        protected MatrixElement<double> Ibr1Neg2Ptr { get; private set; }

        /// <summary>
        /// Gets the (right branch, left branch) element.
        /// </summary>
        protected MatrixElement<double> Ibr2Ibr1Ptr { get; private set; }

        /// <summary>
        /// Gets the right (branch, branch) element.
        /// </summary>
        protected MatrixElement<double> Ibr2Ibr2Ptr { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        protected BaseSimulationState State { get; private set; }

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
        /// Connect the behavior in the circuit
        /// </summary>
        /// <param name="pins">Pin indices in order</param>
        public void Connect(params int[] pins)
        {
            pins.ThrowIfNot(nameof(pins), 4);
            Pos1 = pins[0];
            Neg1 = pins[1];
            Pos2 = pins[2];
            Neg2 = pins[3];
        }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The data provider.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Get parameters
            BaseParameters = context.GetParameterSet<BaseParameters>();

            if (context is ComponentBindingContext cc)
            {
                Pos1 = cc.Pins[0];
                Neg1 = cc.Pins[1];
                Pos2 = cc.Pins[2];
                Neg2 = cc.Pins[3];
            }

            State = ((BaseSimulation)simulation).RealState;
            var solver = State.Solver;
            var variables = simulation.Variables;

            Internal1 = variables.Create(Name.Combine("int1")).Index;
            Internal2 = variables.Create(Name.Combine("int2")).Index;
            BranchEq1 = variables.Create(Name.Combine("branch1"), VariableType.Current).Index;
            BranchEq2 = variables.Create(Name.Combine("branch2"), VariableType.Current).Index;

            Pos1Pos1Ptr = solver.GetMatrixElement(Pos1, Pos1);
            Pos1Int1Ptr = solver.GetMatrixElement(Pos1, Internal1);
            Int1Pos1Ptr = solver.GetMatrixElement(Internal1, Pos1);
            Int1Int1Ptr = solver.GetMatrixElement(Internal1, Internal1);
            Int1Ibr1Ptr = solver.GetMatrixElement(Internal1, BranchEq1);
            Ibr1Int1Ptr = solver.GetMatrixElement(BranchEq1, Internal1);
            Neg1Ibr1Ptr = solver.GetMatrixElement(Neg1, BranchEq1);
            Ibr1Neg1Ptr = solver.GetMatrixElement(BranchEq1, Neg1);

            Pos2Pos2Ptr = solver.GetMatrixElement(Pos2, Pos2);
            Pos2Int2Ptr = solver.GetMatrixElement(Pos2, Internal2);
            Int2Pos2Ptr = solver.GetMatrixElement(Internal2, Pos2);
            Int2Int2Ptr = solver.GetMatrixElement(Internal2, Internal2);
            Int2Ibr2Ptr = solver.GetMatrixElement(Internal2, BranchEq2);
            Ibr2Int2Ptr = solver.GetMatrixElement(BranchEq2, Internal2);
            Neg2Ibr2Ptr = solver.GetMatrixElement(Neg2, BranchEq2);
            Ibr2Neg2Ptr = solver.GetMatrixElement(BranchEq2, Neg2);

            // These pointers are only used to calculate the DC operating point
            Ibr1Pos1Ptr = solver.GetMatrixElement(BranchEq1, Pos1);
            Ibr1Pos2Ptr = solver.GetMatrixElement(BranchEq1, Pos2);
            Ibr1Neg2Ptr = solver.GetMatrixElement(BranchEq1, Neg2);
            Ibr2Ibr1Ptr = solver.GetMatrixElement(BranchEq2, BranchEq1);
            Ibr2Ibr2Ptr = solver.GetMatrixElement(BranchEq2, BranchEq2);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            State = null;
            Pos1Pos1Ptr = null;
            Pos1Int1Ptr = null;
            Int1Pos1Ptr = null;
            Int1Int1Ptr = null;
            Int1Ibr1Ptr = null;
            Ibr1Int1Ptr = null;
            Neg1Ibr1Ptr = null;
            Ibr1Neg1Ptr = null;
            Pos2Pos2Ptr = null;
            Pos2Int2Ptr = null;
            Int2Pos2Ptr = null;
            Int2Int2Ptr = null;
            Int2Ibr2Ptr = null;
            Ibr2Int2Ptr = null;
            Neg2Ibr2Ptr = null;
            Ibr2Neg2Ptr = null;
            Ibr1Pos1Ptr = null;
            Ibr1Pos2Ptr = null;
            Ibr1Neg2Ptr = null;
            Ibr2Ibr1Ptr = null;
            Ibr2Ibr2Ptr = null;
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            // Admittance between POS1 and INT1
            Pos1Pos1Ptr.Value += BaseParameters.Admittance;
            Pos1Int1Ptr.Value -= BaseParameters.Admittance;
            Int1Pos1Ptr.Value -= BaseParameters.Admittance;
            Int1Int1Ptr.Value += BaseParameters.Admittance;

            // Admittance between POS2 and INT2
            Pos2Pos2Ptr.Value += BaseParameters.Admittance;
            Pos2Int2Ptr.Value -= BaseParameters.Admittance;
            Int2Pos2Ptr.Value -= BaseParameters.Admittance;
            Int2Int2Ptr.Value += BaseParameters.Admittance;

            // Add the currents to the positive and negative nodes
            Int1Ibr1Ptr.Value += 1.0;
            Neg1Ibr1Ptr.Value -= 1.0;
            Int2Ibr2Ptr.Value += 1.0;
            Neg2Ibr2Ptr.Value -= 1.0;

            if (State.UseDc)
            {
                // Assume DC operation
                
                // VPOS1 - VNEG1 = VPOS2 - VNEG2
                Ibr1Pos1Ptr.Value += 1.0;
                Ibr1Neg1Ptr.Value -= 1.0;
                Ibr1Pos2Ptr.Value -= 1.0;
                Ibr1Neg2Ptr.Value += 1.0;

                // IBR1 = -IBR2
                Ibr2Ibr1Ptr.Value += 1.0;
                Ibr2Ibr2Ptr.Value += 1.0;
            }
            else
            {
                // INT1-NEG1 voltage source
                Ibr1Int1Ptr.Value += 1.0;
                Ibr1Neg1Ptr.Value -= 1.0;

                // INT2-NEG2 voltage source
                Ibr2Int2Ptr.Value += 1.0;
                Ibr2Neg2Ptr.Value -= 1.0;
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
