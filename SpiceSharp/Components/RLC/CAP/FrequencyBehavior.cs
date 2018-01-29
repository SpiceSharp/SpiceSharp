using System;
using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Capacitor"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary paramters and behaviors
        /// </summary>
        BaseParameters bp;

        /// <summary>
        /// Nodes
        /// </summary>
        int posourceNode, negateNode;
        protected MatrixElement PosPosPtr { get; private set; }
        protected MatrixElement NegNegPtr { get; private set; }
        protected MatrixElement PosNegPtr { get; private set; }
        protected MatrixElement NegPosPtr { get; private set; }

        [PropertyName("v"), PropertyInfo("Capacitor voltage")]
        public Complex GetVoltage(State state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            return state.ComplexSolution[posourceNode] - state.ComplexSolution[negateNode];
        }
        [PropertyName("i"), PropertyName("c"), PropertyInfo("Capacitor current")]
        public Complex GetCurrent(State state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            Complex conductance = state.Laplace * bp.Capacitance.Value;
            return (state.ComplexSolution[posourceNode] - state.ComplexSolution[negateNode]) * conductance;
        }
        [PropertyName("p"), PropertyInfo("Capacitor power")]
        public Complex GetPower(State state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            Complex conductance = state.Laplace * bp.Capacitance.Value;
            Complex voltage = state.ComplexSolution[posourceNode] - state.ComplexSolution[negateNode];
            return voltage * Complex.Conjugate(voltage * conductance);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
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
        /// Connect behavior
        /// </summary>
        /// <param name="pins"></param>
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
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">The matrix</param>
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
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.State;
            var val = state.Laplace * bp.Capacitance.Value;

            // Load the matrix
            PosPosPtr.Add(val);
            NegNegPtr.Add(val);
            PosNegPtr.Sub(val);
            NegPosPtr.Sub(val);
        }
    }
}
