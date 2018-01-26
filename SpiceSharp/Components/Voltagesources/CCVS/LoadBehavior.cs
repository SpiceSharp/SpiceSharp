using SpiceSharp.Attributes;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.CurrentControlledVoltagesourceBehaviors
{
    /// <summary>
    /// General behavior for <see cref="CurrentControlledVoltagesource"/>
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
        public double GetCurrent(State state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[CCVSbranch];
        }
        [PropertyName("v"), PropertyInfo("Output voltage")]
        public double GetVoltage(State state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[CCVSposNode] - state.Solution[CCVSnegNode];
        }
        [PropertyName("p"), PropertyInfo("Power")]
        public double GetPower(State state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[CCVSbranch] * (state.Solution[CCVSposNode] - state.Solution[CCVSnegNode]);
        }

        /// <summary>
        /// Nodes
        /// </summary>
        int CCVSposNode, CCVSnegNode, CCVScontBranch;
        public int CCVSbranch { get; private set; }
        protected MatrixElement CCVSposIbrptr { get; private set; }
        protected MatrixElement CCVSnegIbrptr { get; private set; }
        protected MatrixElement CCVSibrPosptr { get; private set; }
        protected MatrixElement CCVSibrNegptr { get; private set; }
        protected MatrixElement CCVSibrContBrptr { get; private set; }

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
                throw new Diagnostics.CircuitException($"Pin count mismatch: 2 pins expected, {pins.Length} given");
            CCVSposNode = pins[0];
            CCVSnegNode = pins[1];
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
            CCVScontBranch = vsrcload.VSRCbranch;
            CCVSbranch = nodes.Create(Name.Grow("#branch"), Node.NodeType.Current).Index;

            // Get matrix pointers
            CCVSposIbrptr = matrix.GetElement(CCVSposNode, CCVSbranch);
            CCVSnegIbrptr = matrix.GetElement(CCVSnegNode, CCVSbranch);
            CCVSibrPosptr = matrix.GetElement(CCVSbranch, CCVSposNode);
            CCVSibrNegptr = matrix.GetElement(CCVSbranch, CCVSnegNode);
            CCVSibrContBrptr = matrix.GetElement(CCVSbranch, CCVScontBranch);
        }
        
        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            CCVSposIbrptr = null;
            CCVSnegIbrptr = null;
            CCVSibrPosptr = null;
            CCVSibrNegptr = null;
            CCVSibrContBrptr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            CCVSposIbrptr.Add(1.0);
            CCVSibrPosptr.Add(1.0);
            CCVSnegIbrptr.Sub(1.0);
            CCVSibrNegptr.Sub(1.0);
            CCVSibrContBrptr.Sub(bp.CCVScoeff);
        }
    }
}
