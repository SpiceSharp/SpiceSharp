using System.Numerics;
using SpiceSharp.Behaviours;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// AC behaviour for a <see cref="MOS1"/>
    /// </summary>
    public class MOS1AcBehaviour : CircuitObjectBehaviourAcLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var mos1 = ComponentTyped<MOS1>();
            var model = mos1.Model as MOS1Model;
            var state = ckt.State;
            var cstate = state.Complex;
            int xnrm, xrev;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, capgs, capgd, capgb, xgs, xgd, xgb, xbd,
                xbs;

            if (mos1.MOS1mode < 0)
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
            EffectiveLength = mos1.MOS1l - 2 * model.MOS1latDiff;
            GateSourceOverlapCap = model.MOS1gateSourceOverlapCapFactor * mos1.MOS1w;
            GateDrainOverlapCap = model.MOS1gateDrainOverlapCapFactor * mos1.MOS1w;
            GateBulkOverlapCap = model.MOS1gateBulkOverlapCapFactor * EffectiveLength;
            capgs = (state.States[0][mos1.MOS1states + MOS1.MOS1capgs] + state.States[0][mos1.MOS1states + MOS1.MOS1capgs] + GateSourceOverlapCap);
            capgd = (state.States[0][mos1.MOS1states + MOS1.MOS1capgd] + state.States[0][mos1.MOS1states + MOS1.MOS1capgd] + GateDrainOverlapCap);
            capgb = (state.States[0][mos1.MOS1states + MOS1.MOS1capgb] + state.States[0][mos1.MOS1states + MOS1.MOS1capgb] + GateBulkOverlapCap);
            xgs = capgs * cstate.Laplace.Imaginary;
            xgd = capgd * cstate.Laplace.Imaginary;
            xgb = capgb * cstate.Laplace.Imaginary;
            xbd = mos1.MOS1capbd * cstate.Laplace.Imaginary;
            xbs = mos1.MOS1capbs * cstate.Laplace.Imaginary;
            /* 
			* load matrix
			*/

            cstate.Matrix[mos1.MOS1gNode, mos1.MOS1gNode] += new Complex(0.0, xgd + xgs + xgb);
            cstate.Matrix[mos1.MOS1bNode, mos1.MOS1bNode] += new Complex(mos1.MOS1gbd + mos1.MOS1gbs, xgb + xbd + xbs);
            cstate.Matrix[mos1.MOS1dNodePrime, mos1.MOS1dNodePrime] += new Complex(mos1.MOS1drainConductance + mos1.MOS1gds + mos1.MOS1gbd + xrev * (mos1.MOS1gm +
                mos1.MOS1gmbs), xgd + xbd);
            cstate.Matrix[mos1.MOS1sNodePrime, mos1.MOS1sNodePrime] += new Complex(mos1.MOS1sourceConductance + mos1.MOS1gds + mos1.MOS1gbs + xnrm * (mos1.MOS1gm +
                mos1.MOS1gmbs), xgs + xbs);
            cstate.Matrix[mos1.MOS1gNode, mos1.MOS1bNode] -= new Complex(0.0, xgb);
            cstate.Matrix[mos1.MOS1gNode, mos1.MOS1dNodePrime] -= new Complex(0.0, xgd);
            cstate.Matrix[mos1.MOS1gNode, mos1.MOS1sNodePrime] -= new Complex(0.0, xgs);
            cstate.Matrix[mos1.MOS1bNode, mos1.MOS1gNode] -= new Complex(0.0, xgb);
            cstate.Matrix[mos1.MOS1bNode, mos1.MOS1dNodePrime] -= new Complex(mos1.MOS1gbd, xbd);
            cstate.Matrix[mos1.MOS1bNode, mos1.MOS1sNodePrime] -= new Complex(mos1.MOS1gbs, xbs);
            cstate.Matrix[mos1.MOS1dNodePrime, mos1.MOS1gNode] += new Complex((xnrm - xrev) * mos1.MOS1gm, -xgd);
            cstate.Matrix[mos1.MOS1dNodePrime, mos1.MOS1bNode] += new Complex(-mos1.MOS1gbd + (xnrm - xrev) * mos1.MOS1gmbs, -xbd);
            cstate.Matrix[mos1.MOS1sNodePrime, mos1.MOS1gNode] -= new Complex((xnrm - xrev) * mos1.MOS1gm, xgs);
            cstate.Matrix[mos1.MOS1sNodePrime, mos1.MOS1bNode] -= new Complex(mos1.MOS1gbs + (xnrm - xrev) * mos1.MOS1gmbs, xbs);
            cstate.Matrix[mos1.MOS1dNode, mos1.MOS1dNode] += mos1.MOS1drainConductance;
            cstate.Matrix[mos1.MOS1sNode, mos1.MOS1sNode] += mos1.MOS1sourceConductance;

            cstate.Matrix[mos1.MOS1dNode, mos1.MOS1dNodePrime] -= mos1.MOS1drainConductance;
            cstate.Matrix[mos1.MOS1sNode, mos1.MOS1sNodePrime] -= mos1.MOS1sourceConductance;

            cstate.Matrix[mos1.MOS1dNodePrime, mos1.MOS1dNode] -= mos1.MOS1drainConductance;

            cstate.Matrix[mos1.MOS1dNodePrime, mos1.MOS1sNodePrime] -= mos1.MOS1gds + xnrm * (mos1.MOS1gm + mos1.MOS1gmbs);

            cstate.Matrix[mos1.MOS1sNodePrime, mos1.MOS1sNode] -= mos1.MOS1sourceConductance;

            cstate.Matrix[mos1.MOS1sNodePrime, mos1.MOS1dNodePrime] -= mos1.MOS1gds + xrev * (mos1.MOS1gm + mos1.MOS1gmbs);
        }
    }
}
