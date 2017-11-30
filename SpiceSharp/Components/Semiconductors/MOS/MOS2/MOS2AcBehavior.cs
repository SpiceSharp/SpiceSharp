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
        public override void Load(Circuit ckt)
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
            mos2.MOS2GgPtr.Add(new Complex(0.0, xgd + xgs + xgb));
            mos2.MOS2BbPtr.Add(new Complex(mos2.MOS2gbd + mos2.MOS2gbs, xgb + xbd + xbs));
            mos2.MOS2DPdpPtr.Add(new Complex(mos2.MOS2drainConductance + mos2.MOS2gds + mos2.MOS2gbd + xrev * (mos2.MOS2gm + mos2.MOS2gmbs), xgd + xbd));
            mos2.MOS2SPspPtr.Add(new Complex(mos2.MOS2sourceConductance + mos2.MOS2gds + mos2.MOS2gbs + xnrm * (mos2.MOS2gm + mos2.MOS2gmbs), xgs + xbs));
            mos2.MOS2GbPtr.Sub(new Complex(0.0, xgb));
            mos2.MOS2GdpPtr.Sub(new Complex(0.0, xgd));
            mos2.MOS2GspPtr.Sub(new Complex(0.0, xgs));
            mos2.MOS2BgPtr.Sub(new Complex(0.0, xgb));
            mos2.MOS2BdpPtr.Sub(new Complex(mos2.MOS2gbd, xbd));
            mos2.MOS2BspPtr.Sub(new Complex(mos2.MOS2gbs, xbs));
            mos2.MOS2DPgPtr.Add(new Complex((xnrm - xrev) * mos2.MOS2gm, -xgd));
            mos2.MOS2DPbPtr.Add(new Complex(-mos2.MOS2gbd + (xnrm - xrev) * mos2.MOS2gmbs, -xbd));
            mos2.MOS2SPgPtr.Sub(new Complex((xnrm - xrev) * mos2.MOS2gm, xgs));
            mos2.MOS2SPbPtr.Sub(new Complex(mos2.MOS2gbs + (xnrm - xrev) * mos2.MOS2gmbs, xbs));
            mos2.MOS2DdPtr.Add(mos2.MOS2drainConductance);
            mos2.MOS2SsPtr.Add(mos2.MOS2sourceConductance);
            mos2.MOS2DdpPtr.Sub(mos2.MOS2drainConductance);
            mos2.MOS2SspPtr.Sub(mos2.MOS2sourceConductance);
            mos2.MOS2DPdPtr.Sub(mos2.MOS2drainConductance);
            mos2.MOS2DPspPtr.Sub(mos2.MOS2gds + xnrm * (mos2.MOS2gm + mos2.MOS2gmbs));
            mos2.MOS2SPsPtr.Sub(mos2.MOS2sourceConductance);
            mos2.MOS2SPdpPtr.Sub(mos2.MOS2gds + xrev * (mos2.MOS2gm + mos2.MOS2gmbs));
        }
    }
}
