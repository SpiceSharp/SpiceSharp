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
        int posourceNode, negateNode, contPosourceNode, contNegateNode;
        protected MatrixElement PosControlPosPtr { get; private set; }
        protected MatrixElement PosControlNegPtr { get; private set; }
        protected MatrixElement NegControlPosPtr { get; private set; }
        protected MatrixElement NegControlNegPtr { get; private set; }

        /// <summary>
        /// Properties
        /// </summary>
        [PropertyName("v"), PropertyInfo("Voltage")]
        public double GetVoltage(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));
            
            return state.Solution[posourceNode] - state.Solution[negateNode];
        }
        [PropertyName("i"), PropertyName("c"), PropertyInfo("Current")]
        public double GetCurrent(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return (state.Solution[posourceNode] - state.Solution[negateNode]) * bp.Coefficient;
        }
        [PropertyName("p"), PropertyInfo("Power")]
        public double GetPower(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            double v = state.Solution[posourceNode] - state.Solution[negateNode];
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
        /// <param name="propertyName">Property name</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(string propertyName)
        {
            // Avoid reflection for common components
            switch (propertyName)
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
                throw new Diagnostics.CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            posourceNode = pins[0];
            negateNode = pins[1];
            contPosourceNode = pins[2];
            contNegateNode = pins[3];
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
            PosControlPosPtr = matrix.GetElement(posourceNode, contPosourceNode);
            PosControlNegPtr = matrix.GetElement(posourceNode, contNegateNode);
            NegControlPosPtr = matrix.GetElement(negateNode, contPosourceNode);
            NegControlNegPtr = matrix.GetElement(negateNode, contNegateNode);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            PosControlPosPtr = null;
            PosControlNegPtr = null;
            NegControlPosPtr = null;
            NegControlNegPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            PosControlPosPtr.Add(bp.Coefficient);
            PosControlNegPtr.Sub(bp.Coefficient);
            NegControlPosPtr.Sub(bp.Coefficient);
            NegControlNegPtr.Add(bp.Coefficient);
        }
    }
}
