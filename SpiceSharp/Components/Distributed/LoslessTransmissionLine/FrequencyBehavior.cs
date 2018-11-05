using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.LosslessTransmissionLineBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseFrequencyBehavior" />
    /// <seealso cref="SpiceSharp.Components.IConnectedBehavior" />
    public class FrequencyBehavior : BaseFrequencyBehavior, IConnectedBehavior
    {
        private BaseParameters _bp;
        private LoadBehavior _load;

        private int _pos1, _neg1, _pos2, _neg2;
        protected MatrixElement<Complex> Pos1Pos1Ptr { get; private set; }
        protected MatrixElement<Complex> Pos1Int1Ptr { get; private set; }
        protected MatrixElement<Complex> Neg1Ibr1Ptr { get; private set; }
        protected MatrixElement<Complex> Pos2Pos2Ptr { get; private set; }
        protected MatrixElement<Complex> Neg2Ibr2Ptr { get; private set; }
        protected MatrixElement<Complex> Int1Pos1Ptr { get; private set; }
        protected MatrixElement<Complex> Int1Int1Ptr { get; private set; }
        protected MatrixElement<Complex> Int1Ibr1Ptr { get; private set; }
        protected MatrixElement<Complex> Int2Int2Ptr { get; private set; }
        protected MatrixElement<Complex> Int2Ibr2Ptr { get; private set; }
        protected MatrixElement<Complex> Ibr1Neg1Ptr { get; private set; }
        protected MatrixElement<Complex> Ibr1Pos2Ptr { get; private set; }
        protected MatrixElement<Complex> Ibr1Neg2Ptr { get; private set; }
        protected MatrixElement<Complex> Ibr1Int1Ptr { get; private set; }
        protected MatrixElement<Complex> Ibr1Ibr2Ptr { get; private set; }
        protected MatrixElement<Complex> Ibr2Pos1Ptr { get; private set; }
        protected MatrixElement<Complex> Ibr2Neg1Ptr { get; private set; }
        protected MatrixElement<Complex> Ibr2Neg2Ptr { get; private set; }
        protected MatrixElement<Complex> Ibr2Int2Ptr { get; private set; }
        protected MatrixElement<Complex> Ibr2Ibr1Ptr { get; private set; }
        protected MatrixElement<Complex> Pos2Int2Ptr { get; private set; }
        protected MatrixElement<Complex> Int2Pos2Ptr { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public FrequencyBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
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
            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();

            // Get behaviors
            _load = provider.GetBehavior<LoadBehavior>();
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
            Pos1Pos1Ptr = solver.GetMatrixElement(_pos1, _pos1);
            Pos1Int1Ptr = solver.GetMatrixElement(_pos1, _load.Internal1);
            Neg1Ibr1Ptr = solver.GetMatrixElement(_neg1, _load.BranchEq1);
            Pos2Pos2Ptr = solver.GetMatrixElement(_pos2, _pos2);
            Neg2Ibr2Ptr = solver.GetMatrixElement(_neg2, _load.BranchEq2);
            Int1Pos1Ptr = solver.GetMatrixElement(_load.Internal1, _pos1);
            Int1Int1Ptr = solver.GetMatrixElement(_load.Internal1, _load.Internal1);
            Int1Ibr1Ptr = solver.GetMatrixElement(_load.Internal1, _load.BranchEq1);
            Int2Int2Ptr = solver.GetMatrixElement(_load.Internal2, _load.Internal2);
            Int2Ibr2Ptr = solver.GetMatrixElement(_load.Internal2, _load.BranchEq2);
            Ibr1Neg1Ptr = solver.GetMatrixElement(_load.BranchEq1, _neg1);
            Ibr1Pos2Ptr = solver.GetMatrixElement(_load.BranchEq1, _pos2);
            Ibr1Neg2Ptr = solver.GetMatrixElement(_load.BranchEq1, _neg2);
            Ibr1Int1Ptr = solver.GetMatrixElement(_load.BranchEq1, _load.Internal1);
            Ibr1Ibr2Ptr = solver.GetMatrixElement(_load.BranchEq1, _load.BranchEq2);
            Ibr2Pos1Ptr = solver.GetMatrixElement(_load.BranchEq2, _pos1);
            Ibr2Neg1Ptr = solver.GetMatrixElement(_load.BranchEq2, _neg1);
            Ibr2Neg2Ptr = solver.GetMatrixElement(_load.BranchEq2, _neg2);
            Ibr2Int2Ptr = solver.GetMatrixElement(_load.BranchEq2, _load.Internal2);
            Ibr2Ibr1Ptr = solver.GetMatrixElement(_load.BranchEq2, _load.BranchEq1);
            Pos2Int2Ptr = solver.GetMatrixElement(_pos2, _load.Internal2);
            Int2Pos2Ptr = solver.GetMatrixElement(_load.Internal2, _pos2);
        }

        public override void Load(FrequencySimulation simulation)
        {
            var laplace = simulation.ComplexState.Laplace;
            var factor = Complex.Exp(-laplace * _bp.Delay.Value);

            Pos1Pos1Ptr.Value += _bp.Admittance;
            Pos1Int1Ptr.Value -= _bp.Admittance;
            Neg1Ibr1Ptr.Value -= 1;
            Pos2Pos2Ptr.Value += _bp.Admittance;
            Neg2Ibr2Ptr.Value -= 1;
            Int1Pos1Ptr.Value -= _bp.Admittance;
            Int1Int1Ptr.Value += _bp.Admittance;
            Int1Ibr1Ptr.Value += 1;
            Int2Int2Ptr.Value += _bp.Admittance;
            Int2Ibr2Ptr.Value += 1;
            Ibr1Neg1Ptr.Value -= 1;
            Ibr1Pos2Ptr.Value -= factor;
            Ibr1Neg2Ptr.Value += factor;
            Ibr1Int1Ptr.Value += 1;
            Ibr1Ibr2Ptr.Value -= factor * _bp.Impedance;
            Ibr2Pos1Ptr.Value -= factor;
            Ibr2Neg1Ptr.Value += factor;
            Ibr2Neg2Ptr.Value -= 1;
            Ibr2Int2Ptr.Value += 1;
            Ibr2Ibr1Ptr.Value -= factor * _bp.Impedance;
            Pos2Int2Ptr.Value -= _bp.Admittance;
            Int2Pos2Ptr.Value -= _bp.Admittance;
        }
    }
}
