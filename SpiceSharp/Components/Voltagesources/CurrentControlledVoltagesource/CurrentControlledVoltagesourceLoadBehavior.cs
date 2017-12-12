using SpiceSharp.Behaviors;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for <see cref="CurrentControlledVoltagesource"/>
    /// </summary>
    public class CurrentControlledVoltagesourceLoadBehavior : CircuitObjectBehaviorLoad
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
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            var ccvs = component as CurrentControlledVoltagesource;

            // Get behaviors
            var vsrcload = GetBehavior<VoltagesourceLoadBehavior>(ccvs.CCVScontSource);

            // Get nodes
            CCVSposNode = ccvs.CCVSposNode;
            CCVSnegNode = ccvs.CCVSnegNode;
            CCVSbranch = CreateNode(ckt, component.Name.Grow("#branch"), CircuitNode.NodeType.Current).Index;
            CCVScontBranch = vsrcload.VSRCbranch;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            CCVSposIbrptr = matrix.GetElement(CCVSposNode, CCVSbranch);
            CCVSnegIbrptr = matrix.GetElement(CCVSnegNode, CCVSbranch);
            CCVSibrPosptr = matrix.GetElement(CCVSbranch, CCVSposNode);
            CCVSibrNegptr = matrix.GetElement(CCVSbranch, CCVSnegNode);
            CCVSibrContBrptr = matrix.GetElement(CCVSbranch, CCVScontBranch);
            return true;
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
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var ccvs = ComponentTyped<CurrentControlledVoltagesource>();
            var rstate = ckt.State;

            CCVSposIbrptr.Add(1.0);
            CCVSibrPosptr.Add(1.0);
            CCVSnegIbrptr.Sub(1.0);
            CCVSibrNegptr.Sub(1.0);
            CCVSibrContBrptr.Sub(CCVScoeff);
        }
    }
}
