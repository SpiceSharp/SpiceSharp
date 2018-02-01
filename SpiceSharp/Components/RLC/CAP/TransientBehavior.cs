using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// General behavior for <see cref="Capacitor"/>
    /// </summary>
    public class TransientBehavior : Behaviors.TransientBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyName("i"), PropertyInfo("Device current")]
        public double Current => QCap.Derivative;
        [PropertyName("p"), PropertyInfo("Instantaneous device power")]
        public double GetPower(RealState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return QCap.Derivative * (state.Solution[posourceNode] - state.Solution[negateNode]);
        }
        [PropertyName("v"), PropertyInfo("Voltage")]
        public double GetVoltage(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            return state.Solution[posourceNode] - state.Solution[negateNode];
        }

        /// <summary>
        /// Nodes and states
        /// </summary>
        int posourceNode, negateNode;
        protected MatrixElement PosPosPtr { get; private set; }
        protected MatrixElement NegNegPtr { get; private set; }
        protected MatrixElement PosNegPtr { get; private set; }
        protected MatrixElement NegPosPtr { get; private set; }
        protected StateDerivative QCap { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the behavior</param>
        public TransientBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
        }
        
        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new Diagnostics.CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            posourceNode = pins[0];
            negateNode = pins[1];
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="states">States</param>
        public override void CreateStates(StatePool states)
        {
			if (states == null)
				throw new ArgumentNullException(nameof(states));

            QCap = states.CreateDerivative();
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            PosPosPtr = matrix.GetElement(posourceNode, posourceNode);
            NegNegPtr = matrix.GetElement(negateNode, negateNode);
            NegPosPtr = matrix.GetElement(negateNode, posourceNode);
            PosNegPtr = matrix.GetElement(posourceNode, negateNode);
        }

        /// <summary>
        /// Calculate the state for DC
        /// </summary>
        /// <param name="simulation"></param>
        public override void GetDCState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            // Calculate the state for DC
            var sol = simulation.RealState.Solution;
            if (bp.InitialCondition.Given)
                QCap.Current = bp.InitialCondition;
            else
                QCap.Current = bp.Capacitance * (sol[posourceNode] - sol[negateNode]);
        }
        
        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            PosPosPtr = null;
            NegNegPtr = null;
            NegPosPtr = null;
            PosNegPtr = null;
        }

        /// <summary>
        /// Execute behavior for DC and Transient analysis
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Transient(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double vcap = state.Solution[posourceNode] - state.Solution[negateNode];

            // Integrate
            QCap.Current = bp.Capacitance * vcap;
            QCap.Integrate();
            double geq = QCap.Jacobian(bp.Capacitance);
            double ceq = QCap.RhsCurrent();

            // Load matrix
            PosPosPtr.Add(geq);
            NegNegPtr.Add(geq);
            PosNegPtr.Sub(geq);
            NegPosPtr.Sub(geq);

            // Load Rhs vector
            state.Rhs[posourceNode] -= ceq;
            state.Rhs[negateNode] += ceq;
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(ref double timestep)
        {
            QCap.LocalTruncationError(ref timestep);
        }
    }
}
