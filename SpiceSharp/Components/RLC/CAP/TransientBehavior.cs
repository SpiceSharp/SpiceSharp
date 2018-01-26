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
        public double GetCurrent() => QCap.Derivative;
        [PropertyName("p"), PropertyInfo("Instantaneous device power")]
        public double GetPower(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return QCap.Derivative * (state.Solution[posNode] - state.Solution[negNode]);
        }
        [PropertyName("v"), PropertyInfo("Voltage")]
        public double GetVoltage(State state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            return state.Solution[posNode] - state.Solution[negNode];
        }

        /// <summary>
        /// Nodes and states
        /// </summary>
        int posNode, negNode;
        MatrixElement PosPosptr;
        MatrixElement NegNegptr;
        MatrixElement PosNegptr;
        MatrixElement NegPosptr;
        StateDerivative QCap;

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
                throw new Diagnostics.CircuitException($"Pin count mismatch: 2 pins expected, {pins.Length} given");
            posNode = pins[0];
            negNode = pins[1];
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="states">States</param>
        public override void CreateStates(StatePool states)
        {
			if (states == null)
				throw new ArgumentNullException(nameof(states));

            QCap = states.Create();
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            PosPosptr = matrix.GetElement(posNode, posNode);
            NegNegptr = matrix.GetElement(negNode, negNode);
            NegPosptr = matrix.GetElement(negNode, posNode);
            PosNegptr = matrix.GetElement(posNode, negNode);
        }

        /// <summary>
        /// Calculate the state for DC
        /// </summary>
        /// <param name="sim"></param>
        public override void GetDCstate(TimeSimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            // Calculate the state for DC
            var sol = sim.State.Solution;
            if (bp.InitialCondition.Given)
                QCap.Value = bp.InitialCondition;
            else
                QCap.Value = bp.Capacitance * (sol[posNode] - sol[negNode]);
        }
        
        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            PosPosptr = null;
            NegNegptr = null;
            NegPosptr = null;
            PosNegptr = null;
        }

        /// <summary>
        /// Execute behavior for DC and Transient analysis
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Transient(TimeSimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            double vcap = state.Solution[posNode] - state.Solution[negNode];

            // Integrate
            QCap.Value = bp.Capacitance * vcap;
            QCap.Integrate();
            double geq = QCap.Jacobian(bp.Capacitance);
            double ceq = QCap.Current();

            // Load matrix
            PosPosptr.Add(geq);
            NegNegptr.Add(geq);
            PosNegptr.Sub(geq);
            NegPosptr.Sub(geq);

            // Load Rhs vector
            state.Rhs[posNode] -= ceq;
            state.Rhs[negNode] += ceq;
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
