using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using System;
using System.Numerics;

namespace SpiceSharp.Components.VoltageControlledCurrentsourceBehaviors
{
    /// <summary>
    /// AC behavior for a <see cref="VoltageControlledCurrentSource"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        
        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode, contPosNode, contNegNode;
        protected MatrixElement PosContPosptr { get; private set; }
        protected MatrixElement PosContNegptr { get; private set; }
        protected MatrixElement NegContPosptr { get; private set; }
        protected MatrixElement NegContNegptr { get; private set; }

        /// <summary>
        /// Properties
        /// </summary>
        [PropertyName("v"), PropertyInfo("Complex voltage")]
        public Complex GetVoltage(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return new Complex(
                state.Solution[posNode] - state.Solution[negNode],
                state.iSolution[posNode] - state.iSolution[negNode]);
        }
        [PropertyName("c"), PropertyName("i"), PropertyInfo("Complex current")]
        public Complex GetCurrent(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return new Complex(
                state.Solution[contPosNode] - state.Solution[contNegNode],
                state.iSolution[contPosNode] - state.iSolution[contNegNode]) * bp.Coefficient.Value;
        }
        [PropertyName("p"), PropertyInfo("Power")]
        public Complex GetPower(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            Complex v = new Complex(
                state.Solution[posNode] - state.Solution[negNode],
                state.iSolution[posNode] - state.iSolution[negNode]);
            Complex i = new Complex(
                state.Solution[contPosNode] - state.Solution[contNegNode],
                state.iSolution[contPosNode] - state.iSolution[contNegNode]) * bp.Coefficient.Value;
            return -v * Complex.Conjugate(i);
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
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new Diagnostics.CircuitException($"Pin count mismatch: 4 pins expected, {pins.Length} given");
            posNode = pins[0];
            negNode = pins[1];
            contPosNode = pins[2];
            contNegNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            PosContPosptr = matrix.GetElement(posNode, contPosNode);
            PosContNegptr = matrix.GetElement(posNode, contNegNode);
            NegContPosptr = matrix.GetElement(negNode, contPosNode);
            NegContNegptr = matrix.GetElement(negNode, contNegNode);
        }
        
        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            PosContPosptr = null;
            PosContNegptr = null;
            NegContPosptr = null;
            NegContNegptr = null;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            PosContPosptr.Add(bp.Coefficient);
            PosContNegptr.Sub(bp.Coefficient);
            NegContPosptr.Sub(bp.Coefficient);
            NegContNegptr.Add(bp.Coefficient);
        }
    }
}
