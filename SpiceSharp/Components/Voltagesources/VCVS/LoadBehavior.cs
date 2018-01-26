using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.VoltageControlledVoltagesourceBehaviors
{
    /// <summary>
    /// General behavior for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;

        /// <summary>
        /// Properties
        /// </summary>
        [PropertyName("i"), PropertyInfo("Output current")]
        public double GetCurrent(State state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[BranchEq];
        }
        [PropertyName("v"), PropertyInfo("Output current")]
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

            return state.Solution[BranchEq] * (state.Solution[posNode] - state.Solution[negNode]);
        }

        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode, contPosNode, contNegNode;
        public int BranchEq { get; private set; }
        protected MatrixElement PosIbrPtr { get; private set; }
        protected MatrixElement NegIbrPtr { get; private set; }
        protected MatrixElement IbrPosPtr { get; private set; }
        protected MatrixElement IbrNegPtr { get; private set; }
        protected MatrixElement IbrControlPosPtr { get; private set; }
        protected MatrixElement IbrControlNegPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create exports
        /// </summary>
        /// <param name="property">Parameter</param>
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
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            BranchEq = nodes.Create(Name.Grow("#branch"), Node.NodeType.Current).Index;
            PosIbrPtr = matrix.GetElement(posNode, BranchEq);
            NegIbrPtr = matrix.GetElement(negNode, BranchEq);
            IbrPosPtr = matrix.GetElement(BranchEq, posNode);
            IbrNegPtr = matrix.GetElement(BranchEq, negNode);
            IbrControlPosPtr = matrix.GetElement(BranchEq, contPosNode);
            IbrControlNegPtr = matrix.GetElement(BranchEq, contNegNode);
        }
        
        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            PosIbrPtr = null;
            NegIbrPtr = null;
            IbrPosPtr = null;
            IbrNegPtr = null;
            IbrControlPosPtr = null;
            IbrControlNegPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            PosIbrPtr.Add(1.0);
            IbrPosPtr.Add(1.0);
            NegIbrPtr.Sub(1.0);
            IbrNegPtr.Sub(1.0);
            IbrControlPosPtr.Sub(bp.Coefficient);
            IbrControlNegPtr.Add(bp.Coefficient);
        }
    }
}
