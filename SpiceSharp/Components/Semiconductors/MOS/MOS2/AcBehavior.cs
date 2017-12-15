using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.MOS2
{
    /// <summary>
    /// AC behaviour for a <see cref="Components.MOS2"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior load;
        private TemperatureBehavior temp;
        private ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        protected int MOS2dNode, MOS2gNode, MOS2sNode, MOS2bNode, MOS2dNodePrime, MOS2sNodePrime;

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement MOS2DdPtr { get; private set; }
        protected MatrixElement MOS2GgPtr { get; private set; }
        protected MatrixElement MOS2SsPtr { get; private set; }
        protected MatrixElement MOS2BbPtr { get; private set; }
        protected MatrixElement MOS2DPdpPtr { get; private set; }
        protected MatrixElement MOS2SPspPtr { get; private set; }
        protected MatrixElement MOS2DdpPtr { get; private set; }
        protected MatrixElement MOS2GbPtr { get; private set; }
        protected MatrixElement MOS2GdpPtr { get; private set; }
        protected MatrixElement MOS2GspPtr { get; private set; }
        protected MatrixElement MOS2SspPtr { get; private set; }
        protected MatrixElement MOS2BdpPtr { get; private set; }
        protected MatrixElement MOS2BspPtr { get; private set; }
        protected MatrixElement MOS2DPspPtr { get; private set; }
        protected MatrixElement MOS2DPdPtr { get; private set; }
        protected MatrixElement MOS2BgPtr { get; private set; }
        protected MatrixElement MOS2DPgPtr { get; private set; }
        protected MatrixElement MOS2SPgPtr { get; private set; }
        protected MatrixElement MOS2SPsPtr { get; private set; }
        protected MatrixElement MOS2DPbPtr { get; private set; }
        protected MatrixElement MOS2SPbPtr { get; private set; }
        protected MatrixElement MOS2SPdpPtr { get; private set; }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var mos2 = component as Components.MOS2;

            // Get behaviors
            load = GetBehavior<LoadBehavior>(component);
            temp = GetBehavior<TemperatureBehavior>(component);
            modeltemp = GetBehavior<ModelTemperatureBehavior>(mos2.Model);

            // Get nodes
            MOS2dNode = mos2.MOS2dNode;
            MOS2gNode = mos2.MOS2gNode;
            MOS2sNode = mos2.MOS2sNode;
            MOS2bNode = mos2.MOS2bNode;
            MOS2dNodePrime = load.MOS2dNodePrime;
            MOS2sNodePrime = load.MOS2sNodePrime;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            MOS2DdPtr = matrix.GetElement(MOS2dNode, MOS2dNode);
            MOS2GgPtr = matrix.GetElement(MOS2gNode, MOS2gNode);
            MOS2SsPtr = matrix.GetElement(MOS2sNode, MOS2sNode);
            MOS2BbPtr = matrix.GetElement(MOS2bNode, MOS2bNode);
            MOS2DPdpPtr = matrix.GetElement(MOS2dNodePrime, MOS2dNodePrime);
            MOS2SPspPtr = matrix.GetElement(MOS2sNodePrime, MOS2sNodePrime);
            MOS2DdpPtr = matrix.GetElement(MOS2dNode, MOS2dNodePrime);
            MOS2GbPtr = matrix.GetElement(MOS2gNode, MOS2bNode);
            MOS2GdpPtr = matrix.GetElement(MOS2gNode, MOS2dNodePrime);
            MOS2GspPtr = matrix.GetElement(MOS2gNode, MOS2sNodePrime);
            MOS2SspPtr = matrix.GetElement(MOS2sNode, MOS2sNodePrime);
            MOS2BdpPtr = matrix.GetElement(MOS2bNode, MOS2dNodePrime);
            MOS2BspPtr = matrix.GetElement(MOS2bNode, MOS2sNodePrime);
            MOS2DPspPtr = matrix.GetElement(MOS2dNodePrime, MOS2sNodePrime);
            MOS2DPdPtr = matrix.GetElement(MOS2dNodePrime, MOS2dNode);
            MOS2BgPtr = matrix.GetElement(MOS2bNode, MOS2gNode);
            MOS2DPgPtr = matrix.GetElement(MOS2dNodePrime, MOS2gNode);
            MOS2SPgPtr = matrix.GetElement(MOS2sNodePrime, MOS2gNode);
            MOS2SPsPtr = matrix.GetElement(MOS2sNodePrime, MOS2sNode);
            MOS2DPbPtr = matrix.GetElement(MOS2dNodePrime, MOS2bNode);
            MOS2SPbPtr = matrix.GetElement(MOS2sNodePrime, MOS2bNode);
            MOS2SPdpPtr = matrix.GetElement(MOS2sNodePrime, MOS2dNodePrime);
        }

        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var cstate = state;
            int xnrm, xrev;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, capgs, capgd, capgb, xgs, xgd, xgb, xbd,
                xbs;

            if (load.MOS2mode < 0)
            {
                xnrm = 0;
                xrev = 1;
            }
            else
            {
                xnrm = 1;
                xrev = 0;
            }
            /* 
			* meyer's model parameters
			*/
            EffectiveLength = temp.MOS2l - 2 * modeltemp.MOS2latDiff;
            GateSourceOverlapCap = modeltemp.MOS2gateSourceOverlapCapFactor * temp.MOS2w;
            GateDrainOverlapCap = modeltemp.MOS2gateDrainOverlapCapFactor * temp.MOS2w;
            GateBulkOverlapCap = modeltemp.MOS2gateBulkOverlapCapFactor * EffectiveLength;
            capgs = (state.States[0][load.MOS2states + LoadBehavior.MOS2capgs] + state.States[0][load.MOS2states + LoadBehavior.MOS2capgs] + GateSourceOverlapCap);
            capgd = (state.States[0][load.MOS2states + LoadBehavior.MOS2capgd] + state.States[0][load.MOS2states + LoadBehavior.MOS2capgd] + GateDrainOverlapCap);
            capgb = (state.States[0][load.MOS2states + LoadBehavior.MOS2capgb] + state.States[0][load.MOS2states + LoadBehavior.MOS2capgb] + GateBulkOverlapCap);
            xgs = capgs * cstate.Laplace.Imaginary;
            xgd = capgd * cstate.Laplace.Imaginary;
            xgb = capgb * cstate.Laplace.Imaginary;
            xbd = load.MOS2capbd * cstate.Laplace.Imaginary;
            xbs = load.MOS2capbs * cstate.Laplace.Imaginary;

            /* 
			 * load matrix
			 */
            MOS2GgPtr.Add(new Complex(0.0, xgd + xgs + xgb));
            MOS2BbPtr.Add(new Complex(load.MOS2gbd + load.MOS2gbs, xgb + xbd + xbs));
            MOS2DPdpPtr.Add(new Complex(temp.MOS2drainConductance + load.MOS2gds + load.MOS2gbd + xrev * (load.MOS2gm + load.MOS2gmbs), xgd + xbd));
            MOS2SPspPtr.Add(new Complex(temp.MOS2sourceConductance + load.MOS2gds + load.MOS2gbs + xnrm * (load.MOS2gm + load.MOS2gmbs), xgs + xbs));
            MOS2GbPtr.Sub(new Complex(0.0, xgb));
            MOS2GdpPtr.Sub(new Complex(0.0, xgd));
            MOS2GspPtr.Sub(new Complex(0.0, xgs));
            MOS2BgPtr.Sub(new Complex(0.0, xgb));
            MOS2BdpPtr.Sub(new Complex(load.MOS2gbd, xbd));
            MOS2BspPtr.Sub(new Complex(load.MOS2gbs, xbs));
            MOS2DPgPtr.Add(new Complex((xnrm - xrev) * load.MOS2gm, -xgd));
            MOS2DPbPtr.Add(new Complex(-load.MOS2gbd + (xnrm - xrev) * load.MOS2gmbs, -xbd));
            MOS2SPgPtr.Sub(new Complex((xnrm - xrev) * load.MOS2gm, xgs));
            MOS2SPbPtr.Sub(new Complex(load.MOS2gbs + (xnrm - xrev) * load.MOS2gmbs, xbs));
            MOS2DdPtr.Add(temp.MOS2drainConductance);
            MOS2SsPtr.Add(temp.MOS2sourceConductance);
            MOS2DdpPtr.Sub(temp.MOS2drainConductance);
            MOS2SspPtr.Sub(temp.MOS2sourceConductance);
            MOS2DPdPtr.Sub(temp.MOS2drainConductance);
            MOS2DPspPtr.Sub(load.MOS2gds + xnrm * (load.MOS2gm + load.MOS2gmbs));
            MOS2SPsPtr.Sub(temp.MOS2sourceConductance);
            MOS2SPdpPtr.Sub(load.MOS2gds + xrev * (load.MOS2gm + load.MOS2gmbs));
        }
    }
}
