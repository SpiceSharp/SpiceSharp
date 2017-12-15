using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.MOS1
{
    /// <summary>
    /// AC behaviour for a <see cref="MOS1"/>
    /// </summary>
    public class AcBehavior : CircuitObjectBehaviorAcLoad
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
        private int MOS1dNode, MOS1gNode, MOS1sNode, MOS1bNode;
        private int MOS1dNodePrime, MOS1sNodePrime;

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement MOS1DdPtr { get; private set; }
        protected MatrixElement MOS1GgPtr { get; private set; }
        protected MatrixElement MOS1SsPtr { get; private set; }
        protected MatrixElement MOS1BbPtr { get; private set; }
        protected MatrixElement MOS1DPdpPtr { get; private set; }
        protected MatrixElement MOS1SPspPtr { get; private set; }
        protected MatrixElement MOS1DdpPtr { get; private set; }
        protected MatrixElement MOS1GbPtr { get; private set; }
        protected MatrixElement MOS1GdpPtr { get; private set; }
        protected MatrixElement MOS1GspPtr { get; private set; }
        protected MatrixElement MOS1SspPtr { get; private set; }
        protected MatrixElement MOS1BdpPtr { get; private set; }
        protected MatrixElement MOS1BspPtr { get; private set; }
        protected MatrixElement MOS1DPspPtr { get; private set; }
        protected MatrixElement MOS1DPdPtr { get; private set; }
        protected MatrixElement MOS1BgPtr { get; private set; }
        protected MatrixElement MOS1DPgPtr { get; private set; }
        protected MatrixElement MOS1SPgPtr { get; private set; }
        protected MatrixElement MOS1SPsPtr { get; private set; }
        protected MatrixElement MOS1DPbPtr { get; private set; }
        protected MatrixElement MOS1SPbPtr { get; private set; }
        protected MatrixElement MOS1SPdpPtr { get; private set; }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var mos1 = component as Components.MOS1;

            // Get behaviors
            load = GetBehavior<LoadBehavior>(component);
            temp = GetBehavior<TemperatureBehavior>(component);
            modeltemp = GetBehavior<ModelTemperatureBehavior>(mos1.Model);

            // Nodes
            MOS1dNode = mos1.MOS1dNode;
            MOS1gNode = mos1.MOS1gNode;
            MOS1sNode = mos1.MOS1sNode;
            MOS1bNode = mos1.MOS1bNode;
            MOS1dNodePrime = load.MOS1dNodePrime;
            MOS1sNodePrime = load.MOS1sNodePrime;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            MOS1DdPtr = matrix.GetElement(MOS1dNode, MOS1dNode);
            MOS1GgPtr = matrix.GetElement(MOS1gNode, MOS1gNode);
            MOS1SsPtr = matrix.GetElement(MOS1sNode, MOS1sNode);
            MOS1BbPtr = matrix.GetElement(MOS1bNode, MOS1bNode);
            MOS1DPdpPtr = matrix.GetElement(MOS1dNodePrime, MOS1dNodePrime);
            MOS1SPspPtr = matrix.GetElement(MOS1sNodePrime, MOS1sNodePrime);
            MOS1DdpPtr = matrix.GetElement(MOS1dNode, MOS1dNodePrime);
            MOS1GbPtr = matrix.GetElement(MOS1gNode, MOS1bNode);
            MOS1GdpPtr = matrix.GetElement(MOS1gNode, MOS1dNodePrime);
            MOS1GspPtr = matrix.GetElement(MOS1gNode, MOS1sNodePrime);
            MOS1SspPtr = matrix.GetElement(MOS1sNode, MOS1sNodePrime);
            MOS1BdpPtr = matrix.GetElement(MOS1bNode, MOS1dNodePrime);
            MOS1BspPtr = matrix.GetElement(MOS1bNode, MOS1sNodePrime);
            MOS1DPspPtr = matrix.GetElement(MOS1dNodePrime, MOS1sNodePrime);
            MOS1DPdPtr = matrix.GetElement(MOS1dNodePrime, MOS1dNode);
            MOS1BgPtr = matrix.GetElement(MOS1bNode, MOS1gNode);
            MOS1DPgPtr = matrix.GetElement(MOS1dNodePrime, MOS1gNode);
            MOS1SPgPtr = matrix.GetElement(MOS1sNodePrime, MOS1gNode);
            MOS1SPsPtr = matrix.GetElement(MOS1sNodePrime, MOS1sNode);
            MOS1DPbPtr = matrix.GetElement(MOS1dNodePrime, MOS1bNode);
            MOS1SPbPtr = matrix.GetElement(MOS1sNodePrime, MOS1bNode);
            MOS1SPdpPtr = matrix.GetElement(MOS1sNodePrime, MOS1dNodePrime);
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var cstate = state;
            int xnrm, xrev;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, capgs, capgd, capgb, xgs, xgd, xgb, xbd,
                xbs;

            if (load.MOS1mode < 0)
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
            EffectiveLength = temp.MOS1l - 2 * modeltemp.MOS1latDiff;
            GateSourceOverlapCap = modeltemp.MOS1gateSourceOverlapCapFactor * temp.MOS1w;
            GateDrainOverlapCap = modeltemp.MOS1gateDrainOverlapCapFactor * temp.MOS1w;
            GateBulkOverlapCap = modeltemp.MOS1gateBulkOverlapCapFactor * EffectiveLength;
            capgs = (state.States[0][load.MOS1states + LoadBehavior.MOS1capgs] + state.States[0][load.MOS1states + LoadBehavior.MOS1capgs] + GateSourceOverlapCap);
            capgd = (state.States[0][load.MOS1states + LoadBehavior.MOS1capgd] + state.States[0][load.MOS1states + LoadBehavior.MOS1capgd] + GateDrainOverlapCap);
            capgb = (state.States[0][load.MOS1states + LoadBehavior.MOS1capgb] + state.States[0][load.MOS1states + LoadBehavior.MOS1capgb] + GateBulkOverlapCap);
            xgs = capgs * cstate.Laplace.Imaginary;
            xgd = capgd * cstate.Laplace.Imaginary;
            xgb = capgb * cstate.Laplace.Imaginary;
            xbd = load.MOS1capbd * cstate.Laplace.Imaginary;
            xbs = load.MOS1capbs * cstate.Laplace.Imaginary;

            /* 
			 * load matrix
			 */
            MOS1GgPtr.Add(new Complex(0.0, xgd + xgs + xgb));
            MOS1BbPtr.Add(new Complex(load.MOS1gbd + load.MOS1gbs, xgb + xbd + xbs));
            MOS1DPdpPtr.Add(new Complex(temp.MOS1drainConductance + load.MOS1gds + load.MOS1gbd + xrev * (load.MOS1gm + load.MOS1gmbs), xgd + xbd));
            MOS1SPspPtr.Add(new Complex(temp.MOS1sourceConductance + load.MOS1gds + load.MOS1gbs + xnrm * (load.MOS1gm + load.MOS1gmbs), xgs + xbs));
            MOS1GbPtr.Sub(new Complex(0.0, xgb));
            MOS1GdpPtr.Sub(new Complex(0.0, xgd));
            MOS1GspPtr.Sub(new Complex(0.0, xgs));
            MOS1BgPtr.Sub(new Complex(0.0, xgb));
            MOS1BdpPtr.Sub(new Complex(load.MOS1gbd, xbd));
            MOS1BspPtr.Sub(new Complex(load.MOS1gbs, xbs));
            MOS1DPgPtr.Add(new Complex((xnrm - xrev) * load.MOS1gm, -xgd));
            MOS1DPbPtr.Add(new Complex(-load.MOS1gbd + (xnrm - xrev) * load.MOS1gmbs, -xbd));
            MOS1SPgPtr.Sub(new Complex((xnrm - xrev) * load.MOS1gm, xgs));
            MOS1SPbPtr.Sub(new Complex(load.MOS1gbs + (xnrm - xrev) * load.MOS1gmbs, xbs));
            MOS1DdPtr.Add(temp.MOS1drainConductance);
            MOS1SsPtr.Add(temp.MOS1sourceConductance);
            MOS1DdpPtr.Sub(temp.MOS1drainConductance);
            MOS1SspPtr.Sub(temp.MOS1sourceConductance);
            MOS1DPdPtr.Sub(temp.MOS1drainConductance);
            MOS1DPspPtr.Sub(load.MOS1gds + xnrm * (load.MOS1gm + load.MOS1gmbs));
            MOS1SPsPtr.Sub(temp.MOS1sourceConductance);
            MOS1SPdpPtr.Sub(load.MOS1gds + xrev * (load.MOS1gm + load.MOS1gmbs));
        }
    }
}
