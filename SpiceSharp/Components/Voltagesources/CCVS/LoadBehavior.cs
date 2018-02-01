using SpiceSharp.Attributes;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.CurrentControlledVoltagesourceBehaviors
{
    /// <summary>
    /// General behavior for <see cref="CurrentControlledVoltageSource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;
        VoltagesourceBehaviors.LoadBehavior vsrcload;

        /// <summary>
        /// Properties
        /// </summary>
        [PropertyName("i"), PropertyInfo("Output current")]
        public double GetCurrent(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[BranchEq];
        }
        [PropertyName("v"), PropertyInfo("Output voltage")]
        public double GetVoltage(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[posourceNode] - state.Solution[negateNode];
        }
        [PropertyName("p"), PropertyInfo("Power")]
        public double GetPower(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[BranchEq] * (state.Solution[posourceNode] - state.Solution[negateNode]);
        }

        /// <summary>
        /// Nodes
        /// </summary>
        int posourceNode, negateNode, contBranchEq;
        public int BranchEq { get; private set; }
        protected MatrixElement PosBranchPtr { get; private set; }
        protected MatrixElement NegBranchPtr { get; private set; }
        protected MatrixElement BranchPosPtr { get; private set; }
        protected MatrixElement BranchNegPtr { get; private set; }
        protected MatrixElement BranchControlBranchPtr { get; private set; }

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
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);

            // Get behaviors
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

            // Create/get nodes
            contBranchEq = vsrcload.BranchEq;
            BranchEq = nodes.Create(Name.Grow("#branch"), Node.NodeType.Current).Index;

            // Get matrix pointers
            PosBranchPtr = matrix.GetElement(posourceNode, BranchEq);
            NegBranchPtr = matrix.GetElement(negateNode, BranchEq);
            BranchPosPtr = matrix.GetElement(BranchEq, posourceNode);
            BranchNegPtr = matrix.GetElement(BranchEq, negateNode);
            BranchControlBranchPtr = matrix.GetElement(BranchEq, contBranchEq);
        }
        
        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            PosBranchPtr = null;
            NegBranchPtr = null;
            BranchPosPtr = null;
            BranchNegPtr = null;
            BranchControlBranchPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            PosBranchPtr.Add(1.0);
            BranchPosPtr.Add(1.0);
            NegBranchPtr.Sub(1.0);
            BranchNegPtr.Sub(1.0);
            BranchControlBranchPtr.Sub(bp.Coefficient);
        }
    }
}
