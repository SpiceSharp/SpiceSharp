using SpiceSharp.Simulations;
using SpiceSharp.Sparse;
using SpiceSharp.Circuits;
using SpiceSharp.Components.VCCS;
using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Behaviors.VCCS
{
    /// <summary>
    /// General behavior for a <see cref="Components.VoltageControlledCurrentsource"/>
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
        /// Properties
        /// </summary>
        [PropertyName("v"), PropertyInfo("Voltage")]
        public double GetVoltage(State state) => state.Solution[VCCSposNode] - state.Solution[VCCSnegNode];
        [PropertyName("i"), PropertyName("c"), PropertyInfo("Current")]
        public double GetCurrent(State state) => (state.Solution[VCCSposNode] - state.Solution[VCCSnegNode]) * bp.VCCScoeff;
        [PropertyName("p"), PropertyInfo("Power")]
        public double GetPower(State state)
        {
            double v = state.Solution[VCCSposNode] - state.Solution[VCCSnegNode];
            return v * v * bp.VCCScoeff;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create export method
        /// </summary>
        /// <param name="property">Property name</param>
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
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            VCCSposContPosptr.Add(bp.VCCScoeff);
            VCCSposContNegptr.Sub(bp.VCCScoeff);
            VCCSnegContPosptr.Sub(bp.VCCScoeff);
            VCCSnegContNegptr.Add(bp.VCCScoeff);
        }
    }
}
