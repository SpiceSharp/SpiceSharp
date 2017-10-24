using System;
using SpiceSharp.Behaviours;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// Temperature behaviour for a <see cref="BSIM1"/>
    /// </summary>
    public class BSIM1TemperatureBehaviour : CircuitObjectBehaviourTemperature
    {
        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var bsim1 = ComponentTyped<BSIM1>();
            var model = bsim1.Model as BSIM1Model;
            double EffChanLength;
            double EffChanWidth;
            double Leff;
            double Weff;
            double CoxWoverL;

            if ((EffChanLength = bsim1.B1l.Value - model.B1deltaL.Value * 1e-6) <= 0)
                throw new CircuitException($"bsim1.B1: mosfet {bsim1.Name}, model {model.Name}: Effective channel length <= 0");
            if ((EffChanWidth = bsim1.B1w.Value - model.B1deltaW.Value * 1e-6) <= 0)
                throw new CircuitException($"bsim1.B1: mosfet {bsim1.Name}, model {model.Name}: Effective channel width <= 0");
            bsim1.B1GDoverlapCap = EffChanWidth * model.B1gateDrainOverlapCap.Value;
            bsim1.B1GSoverlapCap = EffChanWidth * model.B1gateSourceOverlapCap.Value;
            bsim1.B1GBoverlapCap = bsim1.B1l.Value * model.B1gateBulkOverlapCap.Value;

            /* process drain series resistance */
            if ((bsim1.B1drainConductance = model.B1sheetResistance.Value * bsim1.B1drainSquares.Value) != 0.0)
            {
                bsim1.B1drainConductance = 1.0 / bsim1.B1drainConductance;
            }

            /* process source series resistance */
            if ((bsim1.B1sourceConductance = model.B1sheetResistance.Value * bsim1.B1sourceSquares.Value) != 0.0)
            {
                bsim1.B1sourceConductance = 1.0 / bsim1.B1sourceConductance;
            }

            Leff = EffChanLength * 1.0e6; /* convert into micron */
            Weff = EffChanWidth * 1.0e6; /* convert into micron */
            CoxWoverL = model.Cox * Weff / Leff; /* F / cm *  * 2 */

            bsim1.B1vfb = model.B1vfb0.Value + model.B1vfbL.Value / Leff + model.B1vfbW.Value / Weff;
            bsim1.B1phi = model.B1phi0.Value + model.B1phiL.Value / Leff + model.B1phiW.Value / Weff;
            bsim1.B1K1 = model.B1K10.Value + model.B1K1L.Value / Leff + model.B1K1W.Value / Weff;
            bsim1.B1K2 = model.B1K20.Value + model.B1K2L.Value / Leff + model.B1K2W.Value / Weff;
            bsim1.B1eta = model.B1eta0.Value + model.B1etaL.Value / Leff + model.B1etaW.Value / Weff;
            bsim1.B1etaB = model.B1etaB0.Value + model.B1etaBl.Value / Leff + model.B1etaBw.Value / Weff;
            bsim1.B1etaD = model.B1etaD0.Value + model.B1etaDl.Value / Leff + model.B1etaDw.Value / Weff;
            bsim1.B1betaZero = model.B1mobZero.Value;
            bsim1.B1betaZeroB = model.B1mobZeroB0.Value + model.B1mobZeroBl.Value / Leff + model.B1mobZeroBw.Value / Weff;
            bsim1.B1ugs = model.B1ugs0.Value + model.B1ugsL.Value / Leff + model.B1ugsW.Value / Weff;
            bsim1.B1ugsB = model.B1ugsB0.Value + model.B1ugsBL.Value / Leff + model.B1ugsBW.Value / Weff;
            bsim1.B1uds = model.B1uds0.Value + model.B1udsL.Value / Leff + model.B1udsW.Value / Weff;
            bsim1.B1udsB = model.B1udsB0.Value + model.B1udsBL.Value / Leff + model.B1udsBW.Value / Weff;
            bsim1.B1udsD = model.B1udsD0.Value + model.B1udsDL.Value / Leff + model.B1udsDW.Value / Weff;
            bsim1.B1betaVdd = model.B1mobVdd0.Value + model.B1mobVddl.Value / Leff + model.B1mobVddw.Value / Weff;
            bsim1.B1betaVddB = model.B1mobVddB0.Value + model.B1mobVddBl.Value / Leff + model.B1mobVddBw.Value / Weff;
            bsim1.B1betaVddD = model.B1mobVddD0.Value + model.B1mobVddDl.Value / Leff + model.B1mobVddDw.Value / Weff;
            bsim1.B1subthSlope = model.B1subthSlope0.Value + model.B1subthSlopeL.Value / Leff + model.B1subthSlopeW.Value / Weff;
            bsim1.B1subthSlopeB = model.B1subthSlopeB0.Value + model.B1subthSlopeBL.Value / Leff + model.B1subthSlopeBW.Value / Weff;
            bsim1.B1subthSlopeD = model.B1subthSlopeD0.Value + model.B1subthSlopeDL.Value / Leff + model.B1subthSlopeDW.Value / Weff;

            if (bsim1.B1phi < 0.1)
                bsim1.B1phi = 0.1;
            if (bsim1.B1K1 < 0.0)
                bsim1.B1K1 = 0.0;
            if (bsim1.B1K2 < 0.0)
                bsim1.B1K2 = 0.0;

            bsim1.B1vt0 = bsim1.B1vfb + bsim1.B1phi + bsim1.B1K1 * Math.Sqrt(bsim1.B1phi) - bsim1.B1K2 * bsim1.B1phi;

            bsim1.B1von = bsim1.B1vt0; /* added for initialization */

            /* process Beta Parameters (unit: A / V *  * 2) */
            bsim1.B1betaZero = bsim1.B1betaZero * CoxWoverL;
            bsim1.B1betaZeroB = bsim1.B1betaZeroB * CoxWoverL;
            bsim1.B1betaVdd = bsim1.B1betaVdd * CoxWoverL;
            bsim1.B1betaVddB = bsim1.B1betaVddB * CoxWoverL;
            bsim1.B1betaVddD = Math.Max(bsim1.B1betaVddD * CoxWoverL, 0.0);
        }
    }
}
