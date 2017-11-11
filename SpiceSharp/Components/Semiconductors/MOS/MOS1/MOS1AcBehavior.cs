using System.Numerics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for a <see cref="MOS1"/>
    /// </summary>
    public class MOS1AcBehavior : CircuitObjectBehaviorAcLoad
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
            var cstate = state;
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
            mos1.MOS1GgPtr.Add(new Complex(0.0, xgd + xgs + xgb));
            mos1.MOS1BbPtr.Add(new Complex(mos1.MOS1gbd + mos1.MOS1gbs, xgb + xbd + xbs));
            mos1.MOS1DPdpPtr.Add(new Complex(mos1.MOS1drainConductance + mos1.MOS1gds + mos1.MOS1gbd + xrev * (mos1.MOS1gm + mos1.MOS1gmbs), xgd + xbd));
            mos1.MOS1SPspPtr.Add(new Complex(mos1.MOS1sourceConductance + mos1.MOS1gds + mos1.MOS1gbs + xnrm * (mos1.MOS1gm + mos1.MOS1gmbs), xgs + xbs));
            mos1.MOS1GbPtr.Sub(new Complex(0.0, xgb));
            mos1.MOS1GdpPtr.Sub(new Complex(0.0, xgd));
            mos1.MOS1GspPtr.Sub(new Complex(0.0, xgs));
            mos1.MOS1BgPtr.Sub(new Complex(0.0, xgb));
            mos1.MOS1BdpPtr.Sub(new Complex(mos1.MOS1gbd, xbd));
            mos1.MOS1BspPtr.Sub(new Complex(mos1.MOS1gbs, xbs));
            mos1.MOS1DPgPtr.Add(new Complex((xnrm - xrev) * mos1.MOS1gm, -xgd));
            mos1.MOS1DPbPtr.Add(new Complex(-mos1.MOS1gbd + (xnrm - xrev) * mos1.MOS1gmbs, -xbd));
            mos1.MOS1SPgPtr.Sub(new Complex((xnrm - xrev) * mos1.MOS1gm, xgs));
            mos1.MOS1SPbPtr.Sub(new Complex(mos1.MOS1gbs + (xnrm - xrev) * mos1.MOS1gmbs, xbs));
            mos1.MOS1DdPtr.Add(mos1.MOS1drainConductance);
            mos1.MOS1SsPtr.Add(mos1.MOS1sourceConductance);
            mos1.MOS1DdpPtr.Sub(mos1.MOS1drainConductance);
            mos1.MOS1SspPtr.Sub(mos1.MOS1sourceConductance);
            mos1.MOS1DPdPtr.Sub(mos1.MOS1drainConductance);
            mos1.MOS1DPspPtr.Sub(mos1.MOS1gds + xnrm * (mos1.MOS1gm + mos1.MOS1gmbs));
            mos1.MOS1SPsPtr.Sub(mos1.MOS1sourceConductance);
            mos1.MOS1SPdpPtr.Sub(mos1.MOS1gds + xrev * (mos1.MOS1gm + mos1.MOS1gmbs));
        }
    }
}
