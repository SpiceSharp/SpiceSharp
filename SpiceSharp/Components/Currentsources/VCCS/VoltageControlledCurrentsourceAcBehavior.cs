using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for a <see cref="VoltageControlledCurrentsource"/>
    /// </summary>
    public class VoltageControlledCurrentsourceAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private VoltageControlledCurrentsourceLoadBehavior load;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("i"), SpiceInfo("Output current")]
        public Complex GetCurrent(Circuit ckt)
        {
            return new Complex(
                ckt.State.Solution[VCCScontPosNode] - ckt.State.Solution[VCCScontNegNode],
                ckt.State.iSolution[VCCScontPosNode] - ckt.State.iSolution[VCCScontNegNode]) * load.VCCScoeff.Value;
        }
        [SpiceName("v"), SpiceInfo("Voltage across output")]
        public Complex GetVoltage(Circuit ckt)
        {
            return new Complex(
                ckt.State.Solution[VCCSposNode] - ckt.State.Solution[VCCSnegNode],
                ckt.State.iSolution[VCCSposNode] - ckt.State.iSolution[VCCSnegNode]);
        }
        [SpiceName("p"), SpiceInfo("Power")]
        public Complex GetPower(Circuit ckt)
        {
            Complex current = new Complex(
                ckt.State.Solution[VCCScontPosNode] - ckt.State.Solution[VCCScontNegNode],
                ckt.State.iSolution[VCCScontPosNode] - ckt.State.iSolution[VCCScontNegNode]) * load.VCCScoeff.Value;
            Complex voltage = new Complex(
                ckt.State.Solution[VCCSposNode] - ckt.State.Solution[VCCSnegNode],
                ckt.State.iSolution[VCCSposNode] - ckt.State.iSolution[VCCSnegNode]);
            return voltage * current;
        }

        /// <summary>
        /// Nodes
        /// </summary>
        private int VCCSposNode, VCCSnegNode, VCCScontPosNode, VCCScontNegNode;
        private MatrixElement VCCSposContPosptr;
        private MatrixElement VCCSposContNegptr;
        private MatrixElement VCCSnegContPosptr;
        private MatrixElement VCCSnegContNegptr;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var vccs = component as VoltageControlledCurrentsource;

            // Get behaviors
            load = GetBehavior<VoltageControlledCurrentsourceLoadBehavior>(component);

            // Get nodes
            VCCSposNode = vccs.VCCSposNode;
            VCCSnegNode = vccs.VCCSnegNode;
            VCCScontPosNode = vccs.VCCScontPosNode;
            VCCScontNegNode = vccs.VCCScontNegNode;

            var matrix = ckt.State.Matrix;
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
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var cstate = ckt.State;
            VCCSposContPosptr.Add(load.VCCScoeff.Value);
            VCCSposContNegptr.Sub(load.VCCScoeff.Value);
            VCCSnegContPosptr.Sub(load.VCCScoeff.Value);
            VCCSnegContNegptr.Add(load.VCCScoeff.Value);
        }
    }
}
