using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for a <see cref="MOS2"/>
    /// </summary>
    public class MOS2AcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private MOS2LoadBehavior load;
        private MOS2TemperatureBehavior temp;
        private MOS2ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            var mos2 = component as MOS2;
            load = GetBehavior<MOS2LoadBehavior>(component);
            temp = GetBehavior<MOS2TemperatureBehavior>(component);
            modeltemp = GetBehavior<MOS2ModelTemperatureBehavior>(mos2.Model);
            return true;
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
            capgs = (state.States[0][load.MOS2states + MOS2LoadBehavior.MOS2capgs] + state.States[0][load.MOS2states + MOS2LoadBehavior.MOS2capgs] + GateSourceOverlapCap);
            capgd = (state.States[0][load.MOS2states + MOS2LoadBehavior.MOS2capgd] + state.States[0][load.MOS2states + MOS2LoadBehavior.MOS2capgd] + GateDrainOverlapCap);
            capgb = (state.States[0][load.MOS2states + MOS2LoadBehavior.MOS2capgb] + state.States[0][load.MOS2states + MOS2LoadBehavior.MOS2capgb] + GateBulkOverlapCap);
            xgs = capgs * cstate.Laplace.Imaginary;
            xgd = capgd * cstate.Laplace.Imaginary;
            xgb = capgb * cstate.Laplace.Imaginary;
            xbd = load.MOS2capbd * cstate.Laplace.Imaginary;
            xbs = load.MOS2capbs * cstate.Laplace.Imaginary;

            /* 
			 * load matrix
			 */
            load.MOS2GgPtr.Add(new Complex(0.0, xgd + xgs + xgb));
            load.MOS2BbPtr.Add(new Complex(load.MOS2gbd + load.MOS2gbs, xgb + xbd + xbs));
            load.MOS2DPdpPtr.Add(new Complex(temp.MOS2drainConductance + load.MOS2gds + load.MOS2gbd + xrev * (load.MOS2gm + load.MOS2gmbs), xgd + xbd));
            load.MOS2SPspPtr.Add(new Complex(temp.MOS2sourceConductance + load.MOS2gds + load.MOS2gbs + xnrm * (load.MOS2gm + load.MOS2gmbs), xgs + xbs));
            load.MOS2GbPtr.Sub(new Complex(0.0, xgb));
            load.MOS2GdpPtr.Sub(new Complex(0.0, xgd));
            load.MOS2GspPtr.Sub(new Complex(0.0, xgs));
            load.MOS2BgPtr.Sub(new Complex(0.0, xgb));
            load.MOS2BdpPtr.Sub(new Complex(load.MOS2gbd, xbd));
            load.MOS2BspPtr.Sub(new Complex(load.MOS2gbs, xbs));
            load.MOS2DPgPtr.Add(new Complex((xnrm - xrev) * load.MOS2gm, -xgd));
            load.MOS2DPbPtr.Add(new Complex(-load.MOS2gbd + (xnrm - xrev) * load.MOS2gmbs, -xbd));
            load.MOS2SPgPtr.Sub(new Complex((xnrm - xrev) * load.MOS2gm, xgs));
            load.MOS2SPbPtr.Sub(new Complex(load.MOS2gbs + (xnrm - xrev) * load.MOS2gmbs, xbs));
            load.MOS2DdPtr.Add(temp.MOS2drainConductance);
            load.MOS2SsPtr.Add(temp.MOS2sourceConductance);
            load.MOS2DdpPtr.Sub(temp.MOS2drainConductance);
            load.MOS2SspPtr.Sub(temp.MOS2sourceConductance);
            load.MOS2DPdPtr.Sub(temp.MOS2drainConductance);
            load.MOS2DPspPtr.Sub(load.MOS2gds + xnrm * (load.MOS2gm + load.MOS2gmbs));
            load.MOS2SPsPtr.Sub(temp.MOS2sourceConductance);
            load.MOS2SPdpPtr.Sub(load.MOS2gds + xrev * (load.MOS2gm + load.MOS2gmbs));
        }
    }
}
