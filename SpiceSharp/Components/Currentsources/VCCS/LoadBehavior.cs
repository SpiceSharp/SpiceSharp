using SpiceSharp.Attributes;
using SpiceSharp.Components;
using SpiceSharp.Sparse;
using SpiceSharp.Circuits;
using SpiceSharp.Components.VCCS;
using System;

namespace SpiceSharp.Behaviors.VCCS
{
    /// <summary>
    /// General behavior for a <see cref="VoltageControlledCurrentsource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
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
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create export method
        /// </summary>
        /// <param name="property">Parameter name</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(string property)
        {
            switch (property)
            {
                case "v": return (State state) => state.Solution[VCCSposNode] - state.Solution[VCCSnegNode];
                case "i":
                case "c": return (State state) => (state.Solution[VCCSposNode] - state.Solution[VCCSnegNode]) * bp.VCCScoeff;
                case "p": return (State state) =>
                    {
                        double current = (state.Solution[VCCScontPosNode] - state.Solution[VCCScontNegNode]) * bp.VCCScoeff;
                        double voltage = (state.Solution[VCCSposNode] - state.Solution[VCCSnegNode]);
                        return voltage * current;
                    };
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
        /// Connect behavior
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
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
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
        /// Execute behavior
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var rstate = ckt.State;
            VCCSposContPosptr.Add(bp.VCCScoeff);
            VCCSposContNegptr.Sub(bp.VCCScoeff);
            VCCSnegContPosptr.Sub(bp.VCCScoeff);
            VCCSnegContNegptr.Add(bp.VCCScoeff);
        }
    }
}
