using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.CCVS
{
    /// <summary>
    /// AC behaviour for <see cref="CurrentControlledVoltagesource"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior load;

        /// <summary>
        /// Nodes
        /// </summary>
        protected int CCVSposNode { get; private set; }
        protected int CCVSnegNode { get; private set; }
        public int CCVSbranch { get; private set; }
        public int CCVScontBranch { get; private set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement CCVSposIbrptr { get; private set; }
        protected MatrixElement CCVSnegIbrptr { get; private set; }
        protected MatrixElement CCVSibrPosptr { get; private set; }
        protected MatrixElement CCVSibrNegptr { get; private set; }
        protected MatrixElement CCVSibrContBrptr { get; private set; }

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
            load = GetBehavior<LoadBehavior>(component);

            // Get nodes
            CCVSposNode = ccvs.CCVSposNode;
            CCVSnegNode = ccvs.CCVSnegNode;
            CCVSbranch = load.CCVSbranch;
            CCVScontBranch = load.CCVScontBranch;

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
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            CCVSposIbrptr.Add(1.0);
            CCVSibrPosptr.Add(1.0);
            CCVSnegIbrptr.Sub(1.0);
            CCVSibrNegptr.Sub(1.0);
            CCVSibrContBrptr.Sub(load.CCVScoeff);
        }
    }
}
