using SpiceSharp.Components;
using SpiceSharp.Attributes;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.CCVS
{
    /// <summary>
    /// General behavior for <see cref="CurrentControlledVoltagesource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("gain"), SpiceInfo("Transresistance (gain)")]
        public Parameter CCVScoeff { get; } = new Parameter();
        [SpiceName("i"), SpiceInfo("Output current")]
        public double GetCurrent(Circuit ckt) => ckt.State.Solution[CCVSbranch];
        [SpiceName("v"), SpiceInfo("Output voltage")]
        public double GetVoltage(Circuit ckt) => ckt.State.Solution[CCVSposNode] - ckt.State.Solution[CCVSnegNode];
        [SpiceName("p"), SpiceInfo("Power")]
        public double GetPower(Circuit ckt) => ckt.State.Solution[CCVSbranch] * (ckt.State.Solution[CCVSposNode] - ckt.State.Solution[CCVSnegNode]);

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement CCVSposIbrptr { get; private set; }
        protected MatrixElement CCVSnegIbrptr { get; private set; }
        protected MatrixElement CCVSibrPosptr { get; private set; }
        protected MatrixElement CCVSibrNegptr { get; private set; }
        protected MatrixElement CCVSibrContBrptr { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        protected int CCVSposNode { get; private set; }
        protected int CCVSnegNode { get; private set; }
        public int CCVSbranch { get; private set; }
        public int CCVScontBranch { get; private set; }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(Entity component, Circuit ckt)
        {
            var ccvs = component as CurrentControlledVoltagesource;

            // Get behaviors
            var vsrcload = GetBehavior<VSRC.LoadBehavior>(ccvs.CCVScontSource);

            // Get nodes
            CCVSposNode = ccvs.CCVSposNode;
            CCVSnegNode = ccvs.CCVSnegNode;
            CCVSbranch = CreateNode(ckt, component.Name.Grow("#branch"), Node.NodeType.Current).Index;
            CCVScontBranch = vsrcload.VSRCbranch;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
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
            CCVSibrContBrptr.Sub(CCVScoeff);
        }
    }
}
