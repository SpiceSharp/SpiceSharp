using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="BJTModel"/>
    /// </summary>
    public class BJTModelTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var model = ComponentTyped<BJTModel>();

            if (!model.BJTtnom.Given)
                model.BJTtnom.Value = ckt.State.NominalTemperature;
            model.fact1 = model.BJTtnom / Circuit.CONSTRefTemp;

            if (!model.BJTleakBEcurrent.Given)
            {
                if (model.BJTc2.Given)
                    model.BJTleakBEcurrent.Value = model.BJTc2 * model.BJTsatCur;
                else
                    model.BJTleakBEcurrent.Value = 0;
            }
            if (!model.BJTleakBCcurrent.Given)
            {
                if (model.BJTc4.Given)
                    model.BJTleakBCcurrent.Value = model.BJTc4 * model.BJTsatCur;
                else
                    model.BJTleakBCcurrent.Value = 0;
            }
            if (!model.BJTminBaseResist.Given)
                model.BJTminBaseResist.Value = model.BJTbaseResist;

            /* 
			 * COMPATABILITY WARNING!
			 * special note:  for backward compatability to much older models, spice 2G
			 * implemented a special case which checked if B - E leakage saturation
			 * current was >1, then it was instead a the B - E leakage saturation current
			 * divided by IS, and multiplied it by IS at this point.  This was not
			 * handled correctly in the 2G code, and there is some question on its 
			 * reasonability, since it is also undocumented, so it has been left out
			 * here.  It could easily be added with 1 line.  (The same applies to the B - C
			 * leakage saturation current).   TQ  6 / 29 / 84
			 */

            if (model.BJTearlyVoltF.Given && model.BJTearlyVoltF != 0)
                model.BJTinvEarlyVoltF = 1 / model.BJTearlyVoltF;
            else
                model.BJTinvEarlyVoltF = 0;
            if (model.BJTrollOffF.Given && model.BJTrollOffF != 0)
                model.BJTinvRollOffF = 1 / model.BJTrollOffF;
            else
                model.BJTinvRollOffF = 0;
            if (model.BJTearlyVoltR.Given && model.BJTearlyVoltR != 0)
                model.BJTinvEarlyVoltR = 1 / model.BJTearlyVoltR;
            else
                model.BJTinvEarlyVoltR = 0;
            if (model.BJTrollOffR.Given && model.BJTrollOffR != 0)
                model.BJTinvRollOffR = 1 / model.BJTrollOffR;
            else
                model.BJTinvRollOffR = 0;
            if (model.BJTcollectorResist.Given && model.BJTcollectorResist != 0)
                model.BJTcollectorConduct = 1 / model.BJTcollectorResist;
            else
                model.BJTcollectorConduct = 0;
            if (model.BJTemitterResist.Given && model.BJTemitterResist != 0)
                model.BJTemitterConduct = 1 / model.BJTemitterResist;
            else
                model.BJTemitterConduct = 0;
            if (model.BJTtransitTimeFVBC.Given && model.BJTtransitTimeFVBC != 0)
                model.BJTtransitTimeVBCFactor = 1 / (model.BJTtransitTimeFVBC * 1.44);
            else
                model.BJTtransitTimeVBCFactor = 0;
            model.BJTexcessPhaseFactor = (model.BJTexcessPhase / (180.0 / Circuit.CONSTPI)) * model.BJTtransitTimeF;
            if (model.BJTdepletionCapCoeff.Given)
            {
                if (model.BJTdepletionCapCoeff > 0.9999)
                {
                    model.BJTdepletionCapCoeff.Value = 0.9999;
                    throw new CircuitException($"model.BJT model {model.Name}, parameter fc limited to 0.9999");
                }
            }
            else
            {
                model.BJTdepletionCapCoeff.Value = .5;
            }
            model.xfc = Math.Log(1 - model.BJTdepletionCapCoeff);
            model.BJTf2 = Math.Exp((1 + model.BJTjunctionExpBE) * model.xfc);
            model.BJTf3 = 1 - model.BJTdepletionCapCoeff * (1 + model.BJTjunctionExpBE);
            model.BJTf6 = Math.Exp((1 + model.BJTjunctionExpBC) * model.xfc);
            model.BJTf7 = 1 - model.BJTdepletionCapCoeff * (1 + model.BJTjunctionExpBC);
        }
    }
}
