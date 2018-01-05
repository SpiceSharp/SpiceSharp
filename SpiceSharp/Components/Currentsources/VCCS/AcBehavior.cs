using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Components.VCCS;
using System;

namespace SpiceSharp.Behaviors.VCCS
{
    /// <summary>
    /// AC behavior for a <see cref="Components.VoltageControlledCurrentsource"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        
        /// <summary>
        /// Nodes
        /// </summary>
        int VCCSposNode, VCCSnegNode, VCCScontPosNode, VCCScontNegNode;
        protected MatrixElement VCCSposContPosptr { get; private set; }
        protected MatrixElement VCCSposContNegptr { get; private set; }
        protected MatrixElement VCCSnegContPosptr { get; private set; }
        protected MatrixElement VCCSnegContNegptr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create export method
        /// </summary>
        /// <param name="state">State</param>
        /// <param name="parameter">Parameter</param>
        /// <returns></returns>
        public override Func<double> CreateGetter(State state, string parameter)
        {
            switch (parameter)
            {
                case "vr": return () => state.Solution[VCCSposNode] - state.Solution[VCCSnegNode];
                case "vi": return () => state.iSolution[VCCSposNode] - state.iSolution[VCCSnegNode];
                case "ir": return () => (state.Solution[VCCScontPosNode] - state.Solution[VCCScontNegNode]) * bp.VCCScoeff;
                case "ii": return () => (state.iSolution[VCCScontPosNode] - state.iSolution[VCCScontNegNode]) * bp.VCCScoeff;
                default: return null;
            }
        }

        /// <summary>
        /// Setup the behavior
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
            VCCSposNode = pins[0];
            VCCSnegNode = pins[1];
            VCCScontPosNode = pins[2];
            VCCScontNegNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            VCCSposContPosptr = matrix.GetElement(VCCSposNode, VCCScontPosNode);
            VCCSposContNegptr = matrix.GetElement(VCCSposNode, VCCScontNegNode);
            VCCSnegContPosptr = matrix.GetElement(VCCSnegNode, VCCScontPosNode);
            VCCSnegContNegptr = matrix.GetElement(VCCSnegNode, VCCScontNegNode);
        }
        
        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            VCCSposContPosptr = null;
            VCCSposContNegptr = null;
            VCCSnegContPosptr = null;
            VCCSnegContNegptr = null;
        }

        /// <summary>
        /// Execute the behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            VCCSposContPosptr.Add(bp.VCCScoeff);
            VCCSposContNegptr.Sub(bp.VCCScoeff);
            VCCSnegContPosptr.Sub(bp.VCCScoeff);
            VCCSnegContNegptr.Add(bp.VCCScoeff);
        }
    }
}
