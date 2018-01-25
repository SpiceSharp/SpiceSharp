using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Components.VCCS;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;
using System;
using System.Numerics;

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
        /// Properties
        /// </summary>
        [PropertyName("v"), PropertyInfo("Complex voltage")]
        public Complex GetVoltage(State state)
        {
            return new Complex(
                state.Solution[VCCSposNode] - state.Solution[VCCSnegNode],
                state.iSolution[VCCSposNode] - state.iSolution[VCCSnegNode]);
        }
        [PropertyName("c"), PropertyName("i"), PropertyInfo("Complex current")]
        public Complex GetCurrent(State state)
        {
            return new Complex(
                state.Solution[VCCScontPosNode] - state.Solution[VCCScontNegNode],
                state.iSolution[VCCScontPosNode] - state.iSolution[VCCScontNegNode]) * bp.VCCScoeff.Value;
        }
        [PropertyName("p"), PropertyInfo("Power")]
        public Complex GetPower(State state)
        {
            Complex v = new Complex(
                state.Solution[VCCSposNode] - state.Solution[VCCSnegNode],
                state.iSolution[VCCSposNode] - state.iSolution[VCCSnegNode]);
            Complex i = new Complex(
                state.Solution[VCCScontPosNode] - state.Solution[VCCScontNegNode],
                state.iSolution[VCCScontPosNode] - state.iSolution[VCCScontNegNode]) * bp.VCCScoeff.Value;
            return -v * Complex.Conjugate(i);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
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
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
            VCCSposContPosptr.Add(bp.VCCScoeff);
            VCCSposContNegptr.Sub(bp.VCCScoeff);
            VCCSnegContPosptr.Sub(bp.VCCScoeff);
            VCCSnegContNegptr.Add(bp.VCCScoeff);
        }
    }
}
