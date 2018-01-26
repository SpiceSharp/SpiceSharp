using System;
using System.Numerics;
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
        int posNode, negNode;
        MatrixElement PosPosPtr;
        MatrixElement NegNegPtr;
        MatrixElement PosNegPtr;
        MatrixElement NegPosPtr;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Export methods for AC behavior
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns></returns>
        public override Func<State, Complex> CreateAcExport(string property)
        {
            switch (property)
            {
                case "v": return (State state) => new Complex(state.Solution[posNode] - state.Solution[negNode], state.iSolution[posNode] - state.iSolution[negNode]);
                case "i": return (State state) =>
                {
                    Complex voltage = new Complex(state.Solution[posNode] - state.Solution[negNode], state.iSolution[posNode] - state.iSolution[negNode]);
                    return state.Laplace * bp.Capacitance.Value * voltage;
                };
                case "p": return (State state) =>
                {
                    Complex voltage = new Complex(state.Solution[posNode] - state.Solution[negNode], state.iSolution[posNode] - state.iSolution[negNode]);
                    Complex current = state.Laplace * bp.Capacitance.Value * voltage;
                    return voltage * Complex.Conjugate(current);
                };
                default: return null;
            }
        }

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
                throw new Diagnostics.CircuitException($"Pin count mismatch: 2 pins expected, {pins.Length} given");
            posNode = pins[0];
            negNode = pins[1];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">The matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));


            PosPosPtr = matrix.GetElement(posNode, posNode);
            NegNegPtr = matrix.GetElement(negNode, negNode);
            NegPosPtr = matrix.GetElement(negNode, posNode);
            PosNegPtr = matrix.GetElement(posNode, negNode);
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
