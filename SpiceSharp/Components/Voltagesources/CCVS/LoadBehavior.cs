using SpiceSharp.Components.CCVS;
using SpiceSharp.Attributes;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using System;

namespace SpiceSharp.Behaviors.CCVS
{
    /// <summary>
    /// General behavior for <see cref="Components.CurrentControlledVoltagesource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;
        VSRC.LoadBehavior vsrcload;

        [SpiceName("i"), SpiceInfo("Output current")]
        public double GetCurrent(Circuit ckt) => ckt.State.Solution[CCVSbranch];
        [SpiceName("v"), SpiceInfo("Output voltage")]
        public double GetVoltage(Circuit ckt) => ckt.State.Solution[CCVSposNode] - ckt.State.Solution[CCVSnegNode];
        [SpiceName("p"), SpiceInfo("Power")]
        public double GetPower(Circuit ckt) => ckt.State.Solution[CCVSbranch] * (ckt.State.Solution[CCVSposNode] - ckt.State.Solution[CCVSnegNode]);

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
        /// <param name="property">Parameter</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(string property)
        {
            switch (property)
            {
                case "v": return (State state) => state.Solution[CCVSposNode] - state.Solution[CCVSnegNode];
                case "i":
                case "c": return (State state) => state.Solution[CCVSbranch];
                case "p": return (State state) => state.Solution[CCVSbranch] * (state.Solution[CCVSposNode] - state.Solution[CCVSnegNode]);
                default: return null;
            }
        }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();

            // Get behaviors
            vsrcload = provider.GetBehavior<VSRC.LoadBehavior>(1);
        }

        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
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
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            CCVSposIbrptr.Add(1.0);
            CCVSibrPosptr.Add(1.0);
            CCVSnegIbrptr.Sub(1.0);
            CCVSibrNegptr.Sub(1.0);
            CCVSibrContBrptr.Sub(bp.CCVScoeff);
        }
    }
}
