using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// Load behavior for a <see cref="Inductor"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode;
        public int BranchEq { get; protected set; }
        
        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement PosIbrPtr { get; private set; }
        protected MatrixElement NegIbrPtr { get; private set; }
        protected MatrixElement IbrNegPtr { get; private set; }
        protected MatrixElement IbrPosPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create export method
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(string property)
        {
            switch (property)
            {
                case "v": return (State state) => state.Solution[posNode] - state.Solution[negNode];
                case "i":
                case "c": return (State state) => state.Solution[BranchEq];
                case "p": return (State state) => (state.Solution[posNode] - state.Solution[negNode]) * state.Solution[BranchEq];
                default: return null;
            }
        }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // We don't need anything, acts like a short circuit
        }

        /// <summary>
        /// Connect
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
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            // Create current equation
            BranchEq = nodes.Create(Name.Grow("#branch"), Node.NodeType.Current).Index;

            // Get matrix pointers
            PosIbrPtr = matrix.GetElement(posNode, BranchEq);
            NegIbrPtr = matrix.GetElement(negNode, BranchEq);
            IbrNegPtr = matrix.GetElement(BranchEq, negNode);
            IbrPosPtr = matrix.GetElement(BranchEq, posNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            PosIbrPtr = null;
            NegIbrPtr = null;
            IbrNegPtr = null;
            IbrPosPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            PosIbrPtr.Add(1.0);
            NegIbrPtr.Sub(1.0);
            IbrPosPtr.Add(1.0);
            IbrNegPtr.Sub(1.0);
        }
    }
}
