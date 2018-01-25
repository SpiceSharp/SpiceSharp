using SpiceSharp.Components.CCVS;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;
using System.Numerics;

namespace SpiceSharp.Behaviors.CCVS
{
    /// <summary>
    /// AC behavior for <see cref="Components.CurrentControlledVoltagesource"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        LoadBehavior load;
        VSRC.LoadBehavior vsrcload;

        /// <summary>
        /// Nodes
        /// </summary>
        int CCVSposNode, CCVSnegNode, CCVSbranch, CCVScontBranch;
        protected MatrixElement CCVSposIbrptr { get; private set; }
        protected MatrixElement CCVSnegIbrptr { get; private set; }
        protected MatrixElement CCVSibrPosptr { get; private set; }
        protected MatrixElement CCVSibrNegptr { get; private set; }
        protected MatrixElement CCVSibrContBrptr { get; private set; }

        [PropertyNameAttribute("v"), PropertyInfoAttribute("Complex voltage")]
        public Complex GetVoltage(State state)
        {
            return new Complex(
                state.Solution[CCVSposNode] - state.Solution[CCVSnegNode],
                state.iSolution[CCVSposNode] - state.iSolution[CCVSnegNode]);
        }
        [PropertyNameAttribute("i"), PropertyNameAttribute("c"), PropertyInfoAttribute("Complex current")]
        public Complex GetCurrent(State state)
        {
            return new Complex(
                state.Solution[CCVSbranch],
                state.iSolution[CCVSbranch]);
        }
        [PropertyNameAttribute("p"), PropertyInfoAttribute("Complex power")]
        public Complex GetPower(State state)
        {
            Complex v = new Complex(
                state.Solution[CCVSposNode] - state.Solution[CCVSnegNode],
                state.iSolution[CCVSposNode] - state.iSolution[CCVSnegNode]);
            Complex i = new Complex(
                state.Solution[CCVSbranch],
                state.iSolution[CCVSbranch]);
            return -v * Complex.Conjugate(i);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>();
            vsrcload = provider.GetBehavior<VSRC.LoadBehavior>(1);
        }

        /// <summary>
        /// Connect
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
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            // Get extra nodes
            CCVScontBranch = vsrcload.VSRCbranch;
            CCVSbranch = load.CCVSbranch;

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
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
            CCVSposIbrptr.Add(1.0);
            CCVSibrPosptr.Add(1.0);
            CCVSnegIbrptr.Sub(1.0);
            CCVSibrNegptr.Sub(1.0);
            CCVSibrContBrptr.Sub(bp.CCVScoeff);
        }
    }
}
