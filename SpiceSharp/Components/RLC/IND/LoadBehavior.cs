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
        int INDposNode, INDnegNode;
        public int INDbrEq { get; protected set; }
        
        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement INDposIbrptr { get; private set; }
        protected MatrixElement INDnegIbrptr { get; private set; }
        protected MatrixElement INDibrNegptr { get; private set; }
        protected MatrixElement INDibrPosptr { get; private set; }

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
                case "v": return (State state) => state.Solution[INDposNode] - state.Solution[INDnegNode];
                case "i":
                case "c": return (State state) => state.Solution[INDbrEq];
                case "p": return (State state) => (state.Solution[INDposNode] - state.Solution[INDnegNode]) * state.Solution[INDbrEq];
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
            INDposNode = pins[0];
            INDnegNode = pins[1];
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
            INDbrEq = nodes.Create(Name.Grow("#branch"), Node.NodeType.Current).Index;

            // Get matrix pointers
            INDposIbrptr = matrix.GetElement(INDposNode, INDbrEq);
            INDnegIbrptr = matrix.GetElement(INDnegNode, INDbrEq);
            INDibrNegptr = matrix.GetElement(INDbrEq, INDnegNode);
            INDibrPosptr = matrix.GetElement(INDbrEq, INDposNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            INDposIbrptr = null;
            INDnegIbrptr = null;
            INDibrNegptr = null;
            INDibrPosptr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            INDposIbrptr.Add(1.0);
            INDnegIbrptr.Sub(1.0);
            INDibrPosptr.Add(1.0);
            INDibrNegptr.Sub(1.0);
        }
    }
}
