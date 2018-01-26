using SpiceSharp.Simulations;
using SpiceSharp.Sparse;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.VoltageControlledCurrentsourceBehaviors
{
    /// <summary>
    /// General behavior for a <see cref="VoltageControlledCurrentSource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
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
        [PropertyName("v"), PropertyInfo("Voltage")]
        public double GetVoltage(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));
            
            return state.Solution[posNode] - state.Solution[negNode];
        }
        [PropertyName("i"), PropertyName("c"), PropertyInfo("Current")]
        public double GetCurrent(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return (state.Solution[posNode] - state.Solution[negNode]) * bp.Coefficient;
        }
        [PropertyName("p"), PropertyInfo("Power")]
        public double GetPower(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            double v = state.Solution[posNode] - state.Solution[negNode];
            return v * v * bp.Coefficient;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create export method
        /// </summary>
        /// <param name="property">Property name</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(string property)
        {
            // Avoid reflection for common components
            switch (property)
            {
                case "v": return GetVoltage;
                case "i":
                case "c": return GetCurrent;
                case "p": return GetPower;
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
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
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
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            PosContPosptr.Add(bp.Coefficient);
            PosContNegptr.Sub(bp.Coefficient);
            NegContPosptr.Sub(bp.Coefficient);
            NegContNegptr.Add(bp.Coefficient);
        }
    }
}
