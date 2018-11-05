using System;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.LosslessTransmissionLineBehaviors
{
    /// <summary>
    /// Load behavior for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseLoadBehavior" />
    /// <seealso cref="SpiceSharp.Components.IConnectedBehavior" />
    public class LoadBehavior : BaseLoadBehavior, IConnectedBehavior
    {
        private BaseParameters _bp;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _pos1, _neg1, _pos2, _neg2;
        public int Internal1 { get; private set; }
        public int Internal2 { get; private set; }
        public int BranchEq1 { get; private set; }
        public int BranchEq2 { get; private set; }
        protected MatrixElement<double> Pos1Pos1Ptr { get; private set; }
        protected MatrixElement<double> Pos1Int1Ptr { get; private set; }
        protected MatrixElement<double> Int1Pos1Ptr { get; private set; }
        protected MatrixElement<double> Int1Int1Ptr { get; private set; }
        protected MatrixElement<double> Int1Ibr1Ptr { get; private set; }
        protected MatrixElement<double> Ibr1Int1Ptr { get; private set; }
        protected MatrixElement<double> Neg1Ibr1Ptr { get; private set; }
        protected MatrixElement<double> Ibr1Neg1Ptr { get; private set; }
        
        protected MatrixElement<double> Pos2Pos2Ptr { get; private set; }
        protected MatrixElement<double> Pos2Int2Ptr { get; private set; }
        protected MatrixElement<double> Int2Pos2Ptr { get; private set; }
        protected MatrixElement<double> Int2Int2Ptr { get; private set; }
        protected MatrixElement<double> Int2Ibr2Ptr { get; private set; }
        protected MatrixElement<double> Ibr2Int2Ptr { get; private set; }
        protected MatrixElement<double> Neg2Ibr2Ptr { get; private set; }
        protected MatrixElement<double> Ibr2Neg2Ptr { get; private set; }

        protected MatrixElement<double> Ibr1Pos1Ptr { get; private set; }
        protected MatrixElement<double> Ibr1Pos2Ptr { get; private set; }
        protected MatrixElement<double> Ibr1Neg2Ptr { get; private set; }
        protected MatrixElement<double> Ibr2Ibr1Ptr { get; private set; }
        protected MatrixElement<double> Ibr2Ibr2Ptr { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public LoadBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Connect the behavior in the circuit
        /// </summary>
        /// <param name="pins">Pin indices in order</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            _pos1 = pins[0];
            _neg1 = pins[1];
            _pos2 = pins[2];
            _neg2 = pins[3];
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The data provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            base.Setup(simulation, provider);
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading. Additional
        /// equations can also be allocated here.
        /// </summary>
        /// <param name="variables">The variable set.</param>
        /// <param name="solver">The solver.</param>
        public override void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            // Allocate branch equations first
            Internal1 = variables.Create(Name.Combine("int1")).Index;
            Internal2 = variables.Create(Name.Combine("int2")).Index;
            BranchEq1 = variables.Create(Name.Combine("branch1"), VariableType.Current).Index;
            BranchEq2 = variables.Create(Name.Combine("branch2"), VariableType.Current).Index;

            Pos1Pos1Ptr = solver.GetMatrixElement(_pos1, _pos1);
            Pos1Int1Ptr = solver.GetMatrixElement(_pos1, Internal1);
            Int1Pos1Ptr = solver.GetMatrixElement(Internal1, _pos1);
            Int1Int1Ptr = solver.GetMatrixElement(Internal1, Internal1);
            Int1Ibr1Ptr = solver.GetMatrixElement(Internal1, BranchEq1);
            Ibr1Int1Ptr = solver.GetMatrixElement(BranchEq1, Internal1);
            Neg1Ibr1Ptr = solver.GetMatrixElement(_neg1, BranchEq1);
            Ibr1Neg1Ptr = solver.GetMatrixElement(BranchEq1, _neg1);

            Pos2Pos2Ptr = solver.GetMatrixElement(_pos2, _pos2);
            Pos2Int2Ptr = solver.GetMatrixElement(_pos2, Internal2);
            Int2Pos2Ptr = solver.GetMatrixElement(Internal2, _pos2);
            Int2Int2Ptr = solver.GetMatrixElement(Internal2, Internal2);
            Int2Ibr2Ptr = solver.GetMatrixElement(Internal2, BranchEq2);
            Ibr2Int2Ptr = solver.GetMatrixElement(BranchEq2, Internal2);
            Neg2Ibr2Ptr = solver.GetMatrixElement(_neg2, BranchEq2);
            Ibr2Neg2Ptr = solver.GetMatrixElement(BranchEq2, _neg2);

            // These pointers are only used to calculate the DC operating point
            Ibr1Pos1Ptr = solver.GetMatrixElement(BranchEq1, _pos1);
            Ibr1Pos2Ptr = solver.GetMatrixElement(BranchEq1, _pos2);
            Ibr1Neg2Ptr = solver.GetMatrixElement(BranchEq1, _neg2);
            Ibr2Ibr1Ptr = solver.GetMatrixElement(BranchEq2, BranchEq1);
            Ibr2Ibr2Ptr = solver.GetMatrixElement(BranchEq2, BranchEq2);
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        public override void Load(BaseSimulation simulation)
        {
            var state = simulation.RealState;

            // Admittance between POS1 and INT1
            Pos1Pos1Ptr.Value += _bp.Admittance;
            Pos1Int1Ptr.Value -= _bp.Admittance;
            Int1Pos1Ptr.Value -= _bp.Admittance;
            Int1Int1Ptr.Value += _bp.Admittance;

            // Admittance between POS2 and INT2
            Pos2Pos2Ptr.Value += _bp.Admittance;
            Pos2Int2Ptr.Value -= _bp.Admittance;
            Int2Pos2Ptr.Value -= _bp.Admittance;
            Int2Int2Ptr.Value += _bp.Admittance;

            // Add the currents to the positive and negative nodes
            Int1Ibr1Ptr.Value += 1.0;
            Neg1Ibr1Ptr.Value -= 1.0;
            Int2Ibr2Ptr.Value += 1.0;
            Neg2Ibr2Ptr.Value -= 1.0;

            if (state.UseDc)
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
    }
}
