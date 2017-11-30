using System.Numerics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="MOS3"/>
    /// </summary>
    public class MOS3AcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var mos3 = ComponentTyped<MOS3>();
            var model = mos3.Model as MOS3Model;
            var state = ckt.State;
            var cstate = state;
            int xnrm, xrev;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, capgs, capgd, capgb, xgs, xgd, xgb, xbd,
                xbs;

            if (mos3.MOS3mode < 0)
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
			 * charge oriented model parameters
			 */
            EffectiveLength = mos3.MOS3l - 2 * model.MOS3latDiff;
            GateSourceOverlapCap = model.MOS3gateSourceOverlapCapFactor * mos3.MOS3w;
            GateDrainOverlapCap = model.MOS3gateDrainOverlapCapFactor * mos3.MOS3w;
            GateBulkOverlapCap = model.MOS3gateBulkOverlapCapFactor * EffectiveLength;

            /* 
			 * meyer"s model parameters
			 */
            capgs = (state.States[0][mos3.MOS3states + MOS3.MOS3capgs] + state.States[0][mos3.MOS3states + MOS3.MOS3capgs] + GateSourceOverlapCap);
            capgd = (state.States[0][mos3.MOS3states + MOS3.MOS3capgd] + state.States[0][mos3.MOS3states + MOS3.MOS3capgd] + GateDrainOverlapCap);
            capgb = (state.States[0][mos3.MOS3states + MOS3.MOS3capgb] + state.States[0][mos3.MOS3states + MOS3.MOS3capgb] + GateBulkOverlapCap);
            xgs = capgs * cstate.Laplace.Imaginary;
            xgd = capgd * cstate.Laplace.Imaginary;
            xgb = capgb * cstate.Laplace.Imaginary;
            xbd = mos3.MOS3capbd * cstate.Laplace.Imaginary;
            xbs = mos3.MOS3capbs * cstate.Laplace.Imaginary;

            /* 
			 * load matrix
			 */
            mos3.MOS3GgPtr.Add(new Complex(0.0, xgd + xgs + xgb));
            mos3.MOS3BbPtr.Add(new Complex(mos3.MOS3gbd + mos3.MOS3gbs, xgb + xbd + xbs));
            mos3.MOS3DPdpPtr.Add(new Complex(mos3.MOS3drainConductance + mos3.MOS3gds + mos3.MOS3gbd + xrev * (mos3.MOS3gm + mos3.MOS3gmbs), xgd + xbd));
            mos3.MOS3SPspPtr.Add(new Complex(mos3.MOS3sourceConductance + mos3.MOS3gds + mos3.MOS3gbs + xnrm * (mos3.MOS3gm + mos3.MOS3gmbs), xgs + xbs));
            mos3.MOS3GbPtr.Sub(new Complex(0.0, xgb));
            mos3.MOS3GdpPtr.Sub(new Complex(0.0, xgd));
            mos3.MOS3GspPtr.Sub(new Complex(0.0, xgs));
            mos3.MOS3BgPtr.Sub(new Complex(0.0, xgb));
            mos3.MOS3BdpPtr.Sub(new Complex(mos3.MOS3gbd, xbd));
            mos3.MOS3BspPtr.Sub(new Complex(mos3.MOS3gbs, xbs));
            mos3.MOS3DPgPtr.Add(new Complex((xnrm - xrev) * mos3.MOS3gm, -xgd));
            mos3.MOS3DPbPtr.Add(new Complex(-mos3.MOS3gbd + (xnrm - xrev) * mos3.MOS3gmbs, -xbd));
            mos3.MOS3SPgPtr.Sub(new Complex((xnrm - xrev) * mos3.MOS3gm, xgs));
            mos3.MOS3SPbPtr.Sub(new Complex(mos3.MOS3gbs + (xnrm - xrev) * mos3.MOS3gmbs, xbs));
            mos3.MOS3DdPtr.Add(mos3.MOS3drainConductance);
            mos3.MOS3SsPtr.Add(mos3.MOS3sourceConductance);
            mos3.MOS3DdpPtr.Sub(mos3.MOS3drainConductance);
            mos3.MOS3SspPtr.Sub(mos3.MOS3sourceConductance);
            mos3.MOS3DPdPtr.Sub(mos3.MOS3drainConductance);
            mos3.MOS3DPspPtr.Sub(mos3.MOS3gds + xnrm * (mos3.MOS3gm + mos3.MOS3gmbs));
            mos3.MOS3SPsPtr.Sub(mos3.MOS3sourceConductance);
            mos3.MOS3SPdpPtr.Sub(mos3.MOS3gds + xrev * (mos3.MOS3gm + mos3.MOS3gmbs));
        }
    }
}
