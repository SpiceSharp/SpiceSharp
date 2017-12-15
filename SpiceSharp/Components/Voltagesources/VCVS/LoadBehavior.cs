using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Parameters;

namespace SpiceSharp.Behaviors.VCVS
{
    /// <summary>
    /// General behaviour for a <see cref="VoltageControlledVoltagesource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("gain"), SpiceInfo("Voltage gain")]
        public Parameter VCVScoeff { get; } = new Parameter();
        [SpiceName("i"), SpiceInfo("Output current")]
        public double GetCurrent(Circuit ckt) => ckt.State.Solution[VCVSbranch];
        [SpiceName("v"), SpiceInfo("Output current")]
        public double GetVoltage(Circuit ckt) => ckt.State.Solution[VCVSposNode] - ckt.State.Solution[VCVSnegNode];
        [SpiceName("p"), SpiceInfo("Power")]
        public double GetPower(Circuit ckt) => ckt.State.Solution[VCVSbranch] * (ckt.State.Solution[VCVSposNode] - ckt.State.Solution[VCVSnegNode]);

        /// <summary>
        /// Nodes
        /// </summary>
        protected int VCVSposNode { get; private set; }
        protected int VCVSnegNode { get; private set; }
        protected int VCVScontPosNode { get; private set; }
        protected int VCVScontNegNode { get; private set; }
        public int VCVSbranch { get; protected set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement VCVSposIbrptr { get; private set; }
        protected MatrixElement VCVSnegIbrptr { get; private set; }
        protected MatrixElement VCVSibrPosptr { get; private set; }
        protected MatrixElement VCVSibrNegptr { get; private set; }
        protected MatrixElement VCVSibrContPosptr { get; private set; }
        protected MatrixElement VCVSibrContNegptr { get; private set; }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var vcvs = component as VoltageControlledVoltagesource;

            // Get nodes
            VCVSposNode = vcvs.VCVSposNode;
            VCVSnegNode = vcvs.VCVSnegNode;
            VCVScontPosNode = vcvs.VCVScontPosNode;
            VCVScontNegNode = vcvs.VCVScontNegNode;
            VCVSbranch = CreateNode(ckt, component.Name.Grow("#branch"), CircuitNode.NodeType.Current).Index;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            VCVSposIbrptr = matrix.GetElement(VCVSposNode, VCVSbranch);
            VCVSnegIbrptr = matrix.GetElement(VCVSnegNode, VCVSbranch);
            VCVSibrPosptr = matrix.GetElement(VCVSbranch, VCVSposNode);
            VCVSibrNegptr = matrix.GetElement(VCVSbranch, VCVSnegNode);
            VCVSibrContPosptr = matrix.GetElement(VCVSbranch, VCVScontPosNode);
            VCVSibrContNegptr = matrix.GetElement(VCVSbranch, VCVScontNegNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            VCVSposIbrptr = null;
            VCVSnegIbrptr = null;
            VCVSibrPosptr = null;
            VCVSibrNegptr = null;
            VCVSibrContPosptr = null;
            VCVSibrContNegptr = null;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var rstate = ckt.State;

            VCVSposIbrptr.Add(1.0);
            VCVSibrPosptr.Add(1.0);
            VCVSnegIbrptr.Sub(1.0);
            VCVSibrNegptr.Sub(1.0);
            VCVSibrContPosptr.Sub(VCVScoeff);
            VCVSibrContNegptr.Add(VCVScoeff);
        }
    }
}
