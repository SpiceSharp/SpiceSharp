using SpiceSharp.Parameters;
using SpiceSharp.Components;
using SpiceSharp.Sparse;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.VCCS
{
    /// <summary>
    /// General behaviour for a <see cref="VoltageControlledCurrentsource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("gain"), SpiceInfo("Transconductance of the source (gain)")]
        public Parameter VCCScoeff { get; } = new Parameter();
        [SpiceName("i"), SpiceInfo("Output current")]
        public double GetCurrent(Circuit ckt)
        {
            return (ckt.State.Solution[VCCScontPosNode] - ckt.State.Solution[VCCScontNegNode]) 
                * VCCScoeff.Value;
        }
        [SpiceName("v"), SpiceInfo("Voltage across output")]
        public double GetVoltage(Circuit ckt)
        {
            return (ckt.State.Solution[VCCSposNode] - ckt.State.Solution[VCCSnegNode]);
        }
        [SpiceName("p"), SpiceInfo("Power")]
        public double GetPower(Circuit ckt)
        {
            double current = (ckt.State.Solution[VCCScontPosNode] - ckt.State.Solution[VCCScontNegNode])
                * VCCScoeff.Value;
            double voltage = (ckt.State.Solution[VCCSposNode] - ckt.State.Solution[VCCSnegNode]);
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
        /// Constructor
        /// </summary>
        public LoadBehavior()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gain">Gain</param>
        public LoadBehavior(double gain)
        {
            VCCScoeff.Set(gain);
        }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(Entity component, Circuit ckt)
        {
            var vccs = component as VoltageControlledCurrentsource;
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
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var rstate = ckt.State;
            VCCSposContPosptr.Add(VCCScoeff.Value);
            VCCSposContNegptr.Sub(VCCScoeff.Value);
            VCCSnegContPosptr.Sub(VCCScoeff.Value);
            VCCSnegContNegptr.Add(VCCScoeff.Value);
        }
    }
}
