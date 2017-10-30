using System.Numerics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for a <see cref="MOS2"/>
    /// </summary>
    public class MOS2AcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var mos2 = ComponentTyped<MOS2>();
            var model = mos2.Model as MOS2Model;
            var state = ckt.State;
            var cstate = state;
            int xnrm, xrev;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, capgs, capgd, capgb, xgs, xgd, xgb, xbd,
                xbs;

            if (mos2.MOS2mode < 0)
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
            EffectiveLength = mos2.MOS2l - 2 * model.MOS2latDiff;
            GateSourceOverlapCap = model.MOS2gateSourceOverlapCapFactor * mos2.MOS2w;
            GateDrainOverlapCap = model.MOS2gateDrainOverlapCapFactor * mos2.MOS2w;
            GateBulkOverlapCap = model.MOS2gateBulkOverlapCapFactor * EffectiveLength;
            capgs = (state.States[0][mos2.MOS2states + MOS2.MOS2capgs] + state.States[0][mos2.MOS2states + MOS2.MOS2capgs] + GateSourceOverlapCap);
            capgd = (state.States[0][mos2.MOS2states + MOS2.MOS2capgd] + state.States[0][mos2.MOS2states + MOS2.MOS2capgd] + GateDrainOverlapCap);
            capgb = (state.States[0][mos2.MOS2states + MOS2.MOS2capgb] + state.States[0][mos2.MOS2states + MOS2.MOS2capgb] + GateBulkOverlapCap);
            xgs = capgs * cstate.Laplace.Imaginary;
            xgd = capgd * cstate.Laplace.Imaginary;
            xgb = capgb * cstate.Laplace.Imaginary;
            xbd = mos2.MOS2capbd * cstate.Laplace.Imaginary;
            xbs = mos2.MOS2capbs * cstate.Laplace.Imaginary;
            /* 
			* load matrix
			*/

            // cstate.Matrix[mos2.MOS2gNode, mos2.MOS2gNode] += new Complex(0.0, xgd + xgs + xgb);
            // cstate.Matrix[mos2.MOS2bNode, mos2.MOS2bNode] += new Complex(mos2.MOS2gbd + mos2.MOS2gbs, xgb + xbd + xbs);
            // cstate.Matrix[mos2.MOS2dNodePrime, mos2.MOS2dNodePrime] += new Complex(mos2.MOS2drainConductance + mos2.MOS2gds + mos2.MOS2gbd + xrev * (mos2.MOS2gm + mos2.MOS2gmbs), xgd + xbd);
            // cstate.Matrix[mos2.MOS2sNodePrime, mos2.MOS2sNodePrime] += new Complex(mos2.MOS2sourceConductance + mos2.MOS2gds + mos2.MOS2gbs + xnrm * (mos2.MOS2gm + mos2.MOS2gmbs), xgs + xbs);
            // cstate.Matrix[mos2.MOS2gNode, mos2.MOS2bNode] -= new Complex(0.0, xgb);
            // cstate.Matrix[mos2.MOS2gNode, mos2.MOS2dNodePrime] -= new Complex(0.0, xgd);
            // cstate.Matrix[mos2.MOS2gNode, mos2.MOS2sNodePrime] -= new Complex(0.0, xgs);
            // cstate.Matrix[mos2.MOS2bNode, mos2.MOS2gNode] -= new Complex(0.0, xgb);
            // cstate.Matrix[mos2.MOS2bNode, mos2.MOS2dNodePrime] -= new Complex(mos2.MOS2gbd, xbd);
            // cstate.Matrix[mos2.MOS2bNode, mos2.MOS2sNodePrime] -= new Complex(mos2.MOS2gbs, xbs);
            // cstate.Matrix[mos2.MOS2dNodePrime, mos2.MOS2gNode] += new Complex((xnrm - xrev) * mos2.MOS2gm, -xgd);
            // cstate.Matrix[mos2.MOS2dNodePrime, mos2.MOS2bNode] += new Complex(-mos2.MOS2gbd + (xnrm - xrev) * mos2.MOS2gmbs, -xbd);
            // cstate.Matrix[mos2.MOS2sNodePrime, mos2.MOS2gNode] -= new Complex((xnrm - xrev) * mos2.MOS2gm, xgs);
            // cstate.Matrix[mos2.MOS2sNodePrime, mos2.MOS2bNode] -= new Complex(mos2.MOS2gbs + (xnrm - xrev) * mos2.MOS2gmbs, xbs);
            // cstate.Matrix[mos2.MOS2dNode, mos2.MOS2dNode] += mos2.MOS2drainConductance;
            // cstate.Matrix[mos2.MOS2sNode, mos2.MOS2sNode] += mos2.MOS2sourceConductance;

            // cstate.Matrix[mos2.MOS2dNode, mos2.MOS2dNodePrime] -= mos2.MOS2drainConductance;
            // cstate.Matrix[mos2.MOS2sNode, mos2.MOS2sNodePrime] -= mos2.MOS2sourceConductance;

            // cstate.Matrix[mos2.MOS2dNodePrime, mos2.MOS2dNode] -= mos2.MOS2drainConductance;

            // cstate.Matrix[mos2.MOS2dNodePrime, mos2.MOS2sNodePrime] -= mos2.MOS2gds + xnrm * (mos2.MOS2gm + mos2.MOS2gmbs);

            // cstate.Matrix[mos2.MOS2sNodePrime, mos2.MOS2sNode] -= mos2.MOS2sourceConductance;

            // cstate.Matrix[mos2.MOS2sNodePrime, mos2.MOS2dNodePrime] -= mos2.MOS2gds + xrev * (mos2.MOS2gm + mos2.MOS2gmbs);
        }
    }
}
