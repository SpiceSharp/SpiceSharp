using System;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors
{
    /// <summary>
    /// Behavior for a <see cref="CurrentControlledCurrentSource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
        /// </summary>
        BaseParameters bp;
        VoltagesourceBehaviors.LoadBehavior vsrcload;

        /// <summary>
        /// Nodes
        /// </summary>
        public int ContBranch { get; protected set; }
        int posNode, negNode;
        protected MatrixElement PosContBrptr { get; private set; }
        protected MatrixElement NegContBrptr { get; private set; }

        /// <summary>
        /// Properties
        /// </summary>
        /// <param name="state">State</param>
        /// <returns></returns>
        [PropertyName("i"), PropertyName("c"), PropertyInfo("Current")]
        public double GetCurrent(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return state.Solution[ContBranch] * bp.Coefficient;
        }
        [PropertyName("v"), PropertyInfo("Voltage")]
        public double GetVoltage(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return state.Solution[posNode] - state.Solution[negNode];
        }
        [PropertyName("p"), PropertyInfo("Power")]
        public double GetPower(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return (state.Solution[posNode] - state.Solution[negNode]) * state.Solution[ContBranch] * bp.Coefficient;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create an export method
        /// </summary>
        /// <param name="property">Property name</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(string property)
        {
            // We avoid using reflection for common components
            switch (property)
            {
                case "c":
                case "i": return GetCurrent;
                case "v": return GetVoltage;
                case "p": return GetPower;
                default: return null;
            }
        }

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

            // Get behaviors (0 = CCCS behaviors, 1 = VSRC behaviors)
            vsrcload = provider.GetBehavior<VoltagesourceBehaviors.LoadBehavior>(1);
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
        /// Get matrix pointers
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            ContBranch = vsrcload.VSRCbranch;
            PosContBrptr = matrix.GetElement(posNode, ContBranch);
            NegContBrptr = matrix.GetElement(negNode, ContBranch);
        }
        
        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            PosContBrptr.Add(bp.Coefficient.Value);
            NegContBrptr.Sub(bp.Coefficient.Value);
        }
    }
}
