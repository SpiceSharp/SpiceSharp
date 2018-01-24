using SpiceSharp.Components.VCVS;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Behaviors.VCVS
{
    /// <summary>
    /// General behavior for a <see cref="Components.VoltageControlledVoltagesource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;

        [SpiceName("i"), SpiceInfo("Output current")]
        public double GetCurrent(State state) => state.Solution[VCVSbranch];
        [SpiceName("v"), SpiceInfo("Output current")]
        public double GetVoltage(State state) => state.Solution[VCVSposNode] - state.Solution[VCVSnegNode];
        [SpiceName("p"), SpiceInfo("Power")]
        public double GetPower(State state) => state.Solution[VCVSbranch] * (state.Solution[VCVSposNode] - state.Solution[VCVSnegNode]);

        /// <summary>
        /// Nodes
        /// </summary>
        int VCVSposNode, VCVSnegNode, VCVScontPosNode, VCVScontNegNode;
        public int VCVSbranch { get; private set; }
        protected MatrixElement VCVSposIbrptr { get; private set; }
        protected MatrixElement VCVSnegIbrptr { get; private set; }
        protected MatrixElement VCVSibrPosptr { get; private set; }
        protected MatrixElement VCVSibrNegptr { get; private set; }
        protected MatrixElement VCVSibrContPosptr { get; private set; }
        protected MatrixElement VCVSibrContNegptr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create exports
        /// </summary>
        /// <param name="property">Parameter</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(string property)
        {
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
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            VCVSposNode = pins[0];
            VCVSnegNode = pins[1];
            VCVScontPosNode = pins[2];
            VCVScontNegNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            VCVSbranch = nodes.Create(Name.Grow("#branch"), Node.NodeType.Current).Index;
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
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            VCVSposIbrptr.Add(1.0);
            VCVSibrPosptr.Add(1.0);
            VCVSnegIbrptr.Sub(1.0);
            VCVSibrNegptr.Sub(1.0);
            VCVSibrContPosptr.Sub(bp.VCVScoeff);
            VCVSibrContNegptr.Add(bp.VCVScoeff);
        }
    }
}
