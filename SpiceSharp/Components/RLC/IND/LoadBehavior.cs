﻿using SpiceSharp.Sparse;
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
        int posourceNode, negateNode;
        public int BranchEq { get; protected set; }
        
        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement PosBranchPtr { get; private set; }
        protected MatrixElement NegBranchPtr { get; private set; }
        protected MatrixElement BranchNegPtr { get; private set; }
        protected MatrixElement BranchPosPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create export method
        /// </summary>
        /// <param name="propertyName">Property</param>
        /// <returns></returns>
        public override Func<RealState, double> CreateExport(string propertyName)
        {
            switch (propertyName)
            {
                case "v": return (RealState state) => state.Solution[posourceNode] - state.Solution[negateNode];
                case "i":
                case "c": return (RealState state) => state.Solution[BranchEq];
                case "p": return (RealState state) => (state.Solution[posourceNode] - state.Solution[negateNode]) * state.Solution[BranchEq];
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
                throw new Diagnostics.CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            posourceNode = pins[0];
            negateNode = pins[1];
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
            PosBranchPtr = matrix.GetElement(posourceNode, BranchEq);
            NegBranchPtr = matrix.GetElement(negateNode, BranchEq);
            BranchNegPtr = matrix.GetElement(BranchEq, negateNode);
            BranchPosPtr = matrix.GetElement(BranchEq, posourceNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            PosBranchPtr = null;
            NegBranchPtr = null;
            BranchNegPtr = null;
            BranchPosPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            PosBranchPtr.Add(1.0);
            NegBranchPtr.Sub(1.0);
            BranchPosPtr.Add(1.0);
            BranchNegPtr.Sub(1.0);
        }
    }
}
