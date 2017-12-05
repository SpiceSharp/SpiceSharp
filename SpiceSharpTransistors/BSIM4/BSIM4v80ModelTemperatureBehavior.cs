using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Transistors;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="BSIM4v80Model"/>
    /// </summary>
    public class BSIM4v80ModelTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Temperature(Circuit ckt)
        {
            var model = ComponentTyped<BSIM4v80Model>();
            double Eg, T0, T1, T2, T3;

            model.Temp = ckt.State.Temperature;
            if (model.BSIM4SbulkJctPotential < 0.1)
            {
                model.BSIM4SbulkJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbs is less than 0.1. Pbs is set to 0.1.");
            }
            if (model.BSIM4SsidewallJctPotential < 0.1)
            {
                model.BSIM4SsidewallJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbsws is less than 0.1. Pbsws is set to 0.1.");
            }
            if (model.BSIM4SGatesidewallJctPotential < 0.1)
            {
                model.BSIM4SGatesidewallJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbswgs is less than 0.1. Pbswgs is set to 0.1.");
            }

            if (model.BSIM4DbulkJctPotential < 0.1)
            {
                model.BSIM4DbulkJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbd is less than 0.1. Pbd is set to 0.1.");
            }
            if (model.BSIM4DsidewallJctPotential < 0.1)
            {
                model.BSIM4DsidewallJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbswd is less than 0.1. Pbswd is set to 0.1.");
            }
            if (model.BSIM4DGatesidewallJctPotential < 0.1)
            {
                model.BSIM4DGatesidewallJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbswgd is less than 0.1. Pbswgd is set to 0.1.");
            }

            if (model.BSIM4mtrlMod.Value == 0)
            {
                if ((model.BSIM4toxe.Given) && (model.BSIM4toxp.Given) && (model.BSIM4dtox.Given) && (model.BSIM4toxe != (model.BSIM4toxp + model.BSIM4dtox)))
                    CircuitWarning.Warning(this, "Warning: toxe, toxp and dtox all given and toxe != toxp + dtox; dtox ignored.");
                else if ((model.BSIM4toxe.Given) && (!model.BSIM4toxp.Given))
                    model.BSIM4toxp.Value = model.BSIM4toxe - model.BSIM4dtox;
                else if ((!model.BSIM4toxe.Given) && (model.BSIM4toxp.Given))
                {
                    model.BSIM4toxe.Value = model.BSIM4toxp + model.BSIM4dtox;
                    if (!model.BSIM4toxm.Given)
                        /* v4.7 */
                        model.BSIM4toxm.Value = model.BSIM4toxe;
                }
            }
            else if (model.BSIM4mtrlCompatMod != 0)
            /* v4.7 */
            {
                T0 = model.BSIM4epsrox / 3.9;
                if ((model.BSIM4eot.Given) && (model.BSIM4toxp.Given) && (model.BSIM4dtox.Given) && (Math.Abs(model.BSIM4eot * T0 - (model.BSIM4toxp + model.BSIM4dtox)) > 1.0e-20))
                {
                    CircuitWarning.Warning(this, "Warning: eot, toxp and dtox all given and eot * EPSROX / 3.9 != toxp + dtox; dtox ignored.");
                }
                else if ((model.BSIM4eot.Given) && (!model.BSIM4toxp.Given))
                    model.BSIM4toxp.Value = T0 * model.BSIM4eot - model.BSIM4dtox;
                else if ((!model.BSIM4eot.Given) && (model.BSIM4toxp.Given))
                {
                    model.BSIM4eot.Value = (model.BSIM4toxp + model.BSIM4dtox) / T0;
                    if (!model.BSIM4toxm.Given)
                        model.BSIM4toxm.Value = model.BSIM4eot;
                }
            }

            if (model.BSIM4mtrlMod != 0)
            {
                model.epsrox = 3.9;
                model.toxe = model.BSIM4eot;
                model.epssub = Transistor.EPS0 * model.BSIM4epsrsub;
            }
            else
            {
                model.epsrox = model.BSIM4epsrox;
                model.toxe = model.BSIM4toxe;
                model.epssub = Transistor.EPSSI;
            }

            model.BSIM4coxe = model.epsrox * Transistor.EPS0 / model.toxe;
            if (model.BSIM4mtrlMod.Value == 0 || model.BSIM4mtrlCompatMod != 0)
                model.BSIM4coxp = model.BSIM4epsrox * Transistor.EPS0 / model.BSIM4toxp;

            if (!model.BSIM4cgdo.Given)
            {
                if (model.BSIM4dlc.Given && (model.BSIM4dlc > 0.0))
                    model.BSIM4cgdo.Value = model.BSIM4dlc * model.BSIM4coxe - model.BSIM4cgdl;
                else
                    model.BSIM4cgdo.Value = 0.6 * model.BSIM4xj * model.BSIM4coxe;
            }
            if (!model.BSIM4cgso.Given)
            {
                if (model.BSIM4dlc.Given && (model.BSIM4dlc > 0.0))
                    model.BSIM4cgso.Value = model.BSIM4dlc * model.BSIM4coxe - model.BSIM4cgsl;
                else
                    model.BSIM4cgso.Value = 0.6 * model.BSIM4xj * model.BSIM4coxe;
            }
            if (!model.BSIM4cgbo.Given)
                model.BSIM4cgbo.Value = 2.0 * model.BSIM4dwc * model.BSIM4coxe;

            model.Tnom = model.BSIM4tnom;
            model.TRatio = model.Temp / model.Tnom;

            model.BSIM4vcrit = Circuit.CONSTvt0 * Math.Log(Circuit.CONSTvt0 / (Circuit.CONSTroot2 * 1.0e-14));
            model.BSIM4factor1 = Math.Sqrt(model.epssub / (model.epsrox * Transistor.EPS0) * model.toxe);

            model.Vtm0 = model.BSIM4vtm0 = Transistor.KboQ * model.Tnom;

            if (model.BSIM4mtrlMod.Value == 0)
            {
                model.Eg0 = 1.16 - 7.02e-4 * model.Tnom * model.Tnom / (model.Tnom + 1108.0);
                model.ni = 1.45e10 * (model.Tnom / 300.15) * Math.Sqrt(model.Tnom / 300.15) * Math.Exp(21.5565981 - model.Eg0 / (2.0 * model.Vtm0));
            }
            else
            {
                model.Eg0 = model.BSIM4bg0sub - model.BSIM4tbgasub * model.Tnom * model.Tnom / (model.Tnom + model.BSIM4tbgbsub);
                T0 = model.BSIM4bg0sub - model.BSIM4tbgasub * 90090.0225 / (300.15 + model.BSIM4tbgbsub);
                model.ni = model.BSIM4ni0sub * (model.Tnom / 300.15) * Math.Sqrt(model.Tnom / 300.15) * Math.Exp((T0 - model.Eg0) / (2.0 * model.Vtm0));
            }

            model.BSIM4Eg0 = model.Eg0;
            model.BSIM4vtm = Transistor.KboQ * model.Temp;
            if (model.BSIM4mtrlMod.Value == 0)
                Eg = 1.16 - 7.02e-4 * model.Temp * model.Temp / (model.Temp + 1108.0);
            else
                Eg = model.BSIM4bg0sub - model.BSIM4tbgasub * model.Temp * model.Temp / (model.Temp + model.BSIM4tbgbsub);
            if (model.Temp != model.Tnom)
            {
                T0 = model.Eg0 / model.Vtm0 - Eg / model.BSIM4vtm;
                T1 = Math.Log(model.Temp / model.Tnom);
                T2 = T0 + model.BSIM4SjctTempExponent * T1;
                T3 = Math.Exp(T2 / model.BSIM4SjctEmissionCoeff);
                model.BSIM4SjctTempSatCurDensity = model.BSIM4SjctSatCurDensity * T3;
                model.BSIM4SjctSidewallTempSatCurDensity = model.BSIM4SjctSidewallSatCurDensity * T3;
                model.BSIM4SjctGateSidewallTempSatCurDensity = model.BSIM4SjctGateSidewallSatCurDensity * T3;

                T2 = T0 + model.BSIM4DjctTempExponent * T1;
                T3 = Math.Exp(T2 / model.BSIM4DjctEmissionCoeff);
                model.BSIM4DjctTempSatCurDensity = model.BSIM4DjctSatCurDensity * T3;
                model.BSIM4DjctSidewallTempSatCurDensity = model.BSIM4DjctSidewallSatCurDensity * T3;
                model.BSIM4DjctGateSidewallTempSatCurDensity = model.BSIM4DjctGateSidewallSatCurDensity * T3;
            }
            else
            {
                model.BSIM4SjctTempSatCurDensity = model.BSIM4SjctSatCurDensity;
                model.BSIM4SjctSidewallTempSatCurDensity = model.BSIM4SjctSidewallSatCurDensity;
                model.BSIM4SjctGateSidewallTempSatCurDensity = model.BSIM4SjctGateSidewallSatCurDensity;
                model.BSIM4DjctTempSatCurDensity = model.BSIM4DjctSatCurDensity;
                model.BSIM4DjctSidewallTempSatCurDensity = model.BSIM4DjctSidewallSatCurDensity;
                model.BSIM4DjctGateSidewallTempSatCurDensity = model.BSIM4DjctGateSidewallSatCurDensity;
            }

            if (model.BSIM4SjctTempSatCurDensity < 0.0)
                model.BSIM4SjctTempSatCurDensity = 0.0;
            if (model.BSIM4SjctSidewallTempSatCurDensity < 0.0)
                model.BSIM4SjctSidewallTempSatCurDensity = 0.0;
            if (model.BSIM4SjctGateSidewallTempSatCurDensity < 0.0)
                model.BSIM4SjctGateSidewallTempSatCurDensity = 0.0;
            if (model.BSIM4DjctTempSatCurDensity < 0.0)
                model.BSIM4DjctTempSatCurDensity = 0.0;
            if (model.BSIM4DjctSidewallTempSatCurDensity < 0.0)
                model.BSIM4DjctSidewallTempSatCurDensity = 0.0;
            if (model.BSIM4DjctGateSidewallTempSatCurDensity < 0.0)
                model.BSIM4DjctGateSidewallTempSatCurDensity = 0.0;

            /* Temperature dependence of D / B and S / B diode capacitance begins */
            model.delTemp = ckt.State.Temperature - model.BSIM4tnom;
            T0 = model.BSIM4tcj * model.delTemp;
            if (T0 >= -1.0)
            {
                model.BSIM4SunitAreaTempJctCap = model.BSIM4SunitAreaJctCap * (1.0 + T0); /* bug_fix - JX */
                model.BSIM4DunitAreaTempJctCap = model.BSIM4DunitAreaJctCap * (1.0 + T0);
            }
            else
            {
                if (model.BSIM4SunitAreaJctCap > 0.0)
                {
                    model.BSIM4SunitAreaTempJctCap = 0.0;
                    CircuitWarning.Warning(this, "Temperature effect has caused cjs to be negative. Cjs is clamped to zero.");
                }
                if (model.BSIM4DunitAreaJctCap > 0.0)
                {
                    model.BSIM4DunitAreaTempJctCap = 0.0;
                    CircuitWarning.Warning(this, "Temperature effect has caused cjd to be negative. Cjd is clamped to zero.");
                }
            }
            T0 = model.BSIM4tcjsw * model.delTemp;
            if (model.BSIM4SunitLengthSidewallJctCap < 0.0)
            /* 4.6.2 */
            {
                model.BSIM4SunitLengthSidewallJctCap.Value = 0.0;
                CircuitWarning.Warning(this, "CJSWS is negative. Cjsws is clamped to zero.");
            }
            if (model.BSIM4DunitLengthSidewallJctCap < 0.0)
            {
                model.BSIM4DunitLengthSidewallJctCap.Value = 0.0;
                CircuitWarning.Warning(this, "CJSWD is negative. Cjswd is clamped to zero.");
            }
            if (T0 >= -1.0)
            {
                model.BSIM4SunitLengthSidewallTempJctCap = model.BSIM4SunitLengthSidewallJctCap * (1.0 + T0);
                model.BSIM4DunitLengthSidewallTempJctCap = model.BSIM4DunitLengthSidewallJctCap * (1.0 + T0);
            }
            else
            {
                if (model.BSIM4SunitLengthSidewallJctCap > 0.0)
                {
                    model.BSIM4SunitLengthSidewallTempJctCap = 0.0;
                    CircuitWarning.Warning(this, "Temperature effect has caused cjsws to be negative. Cjsws is clamped to zero.");
                }
                if (model.BSIM4DunitLengthSidewallJctCap > 0.0)
                {
                    model.BSIM4DunitLengthSidewallTempJctCap = 0.0;
                    CircuitWarning.Warning(this, "Temperature effect has caused cjswd to be negative. Cjswd is clamped to zero.");
                }
            }
            T0 = model.BSIM4tcjswg * model.delTemp;
            if (T0 >= -1.0)
            {
                model.BSIM4SunitLengthGateSidewallTempJctCap = model.BSIM4SunitLengthGateSidewallJctCap * (1.0 + T0);
                model.BSIM4DunitLengthGateSidewallTempJctCap = model.BSIM4DunitLengthGateSidewallJctCap * (1.0 + T0);
            }
            else
            {
                if (model.BSIM4SunitLengthGateSidewallJctCap > 0.0)
                {
                    model.BSIM4SunitLengthGateSidewallTempJctCap = 0.0;
                    CircuitWarning.Warning(this, "Temperature effect has caused cjswgs to be negative. Cjswgs is clamped to zero.");
                }
                if (model.BSIM4DunitLengthGateSidewallJctCap > 0.0)
                {
                    model.BSIM4DunitLengthGateSidewallTempJctCap = 0.0;
                    CircuitWarning.Warning(this, "Temperature effect has caused cjswgd to be negative. Cjswgd is clamped to zero.");
                }
            }

            model.BSIM4PhiBS = model.BSIM4SbulkJctPotential - model.BSIM4tpb * model.delTemp;
            if (model.BSIM4PhiBS < 0.01)
            {
                model.BSIM4PhiBS = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbs to be less than 0.01. Pbs is clamped to 0.01.");
            }
            model.BSIM4PhiBD = model.BSIM4DbulkJctPotential - model.BSIM4tpb * model.delTemp;
            if (model.BSIM4PhiBD < 0.01)
            {
                model.BSIM4PhiBD = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbd to be less than 0.01. Pbd is clamped to 0.01.");
            }

            model.BSIM4PhiBSWS = model.BSIM4SsidewallJctPotential - model.BSIM4tpbsw * model.delTemp;
            if (model.BSIM4PhiBSWS <= 0.01)
            {
                model.BSIM4PhiBSWS = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbsws to be less than 0.01. Pbsws is clamped to 0.01.");
            }
            model.BSIM4PhiBSWD = model.BSIM4DsidewallJctPotential - model.BSIM4tpbsw * model.delTemp;
            if (model.BSIM4PhiBSWD <= 0.01)
            {
                model.BSIM4PhiBSWD = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbswd to be less than 0.01. Pbswd is clamped to 0.01.");
            }

            model.BSIM4PhiBSWGS = model.BSIM4SGatesidewallJctPotential - model.BSIM4tpbswg * model.delTemp;
            if (model.BSIM4PhiBSWGS <= 0.01)
            {
                model.BSIM4PhiBSWGS = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbswgs to be less than 0.01. Pbswgs is clamped to 0.01.");
            }
            model.BSIM4PhiBSWGD = model.BSIM4DGatesidewallJctPotential - model.BSIM4tpbswg * model.delTemp;
            if (model.BSIM4PhiBSWGD <= 0.01)
            {
                model.BSIM4PhiBSWGD = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbswgd to be less than 0.01. Pbswgd is clamped to 0.01.");
            } /* End of junction capacitance */

            if (model.BSIM4ijthdfwd <= 0.0)
            {
                model.BSIM4ijthdfwd.Value = 0.0;
                CircuitWarning.Warning(this, $"Ijthdfwd reset to {model.BSIM4ijthdfwd}.");
            }
            if (model.BSIM4ijthsfwd <= 0.0)
            {
                model.BSIM4ijthsfwd.Value = 0.0;
                CircuitWarning.Warning(this, $"Ijthsfwd reset to {model.BSIM4ijthsfwd}.");
            }
            if (model.BSIM4ijthdrev <= 0.0)
            {
                model.BSIM4ijthdrev.Value = 0.0;
                CircuitWarning.Warning(this, $"Ijthdrev reset to {model.BSIM4ijthdrev}.");
            }
            if (model.BSIM4ijthsrev <= 0.0)
            {
                model.BSIM4ijthsrev.Value = 0.0;
                CircuitWarning.Warning(this, $"Ijthsrev reset to {model.BSIM4ijthsrev}.");
            }

            if ((model.BSIM4xjbvd <= 0.0) && (model.BSIM4dioMod.Value == 2))
            {
                model.BSIM4xjbvd.Value = 0.0;
                CircuitWarning.Warning(this, $"Xjbvd reset to {model.BSIM4xjbvd}.");
            }
            else if ((model.BSIM4xjbvd < 0.0) && (model.BSIM4dioMod.Value == 0))
            {
                model.BSIM4xjbvd.Value = 0.0;
                CircuitWarning.Warning(this, $"Xjbvd reset to {model.BSIM4xjbvd}.");
            }

            if (model.BSIM4bvd <= 0.0)
            /* 4.6.2 */
            {
                model.BSIM4bvd.Value = 0.0;
                CircuitWarning.Warning(this, $"BVD reset to {model.BSIM4bvd}.");
            }

            if ((model.BSIM4xjbvs <= 0.0) && (model.BSIM4dioMod.Value == 2))
            {
                model.BSIM4xjbvs.Value = 0.0;
                CircuitWarning.Warning(this, $"Xjbvs reset to {model.BSIM4xjbvs}.");
            }
            else if ((model.BSIM4xjbvs < 0.0) && (model.BSIM4dioMod.Value == 0))
            {
                model.BSIM4xjbvs.Value = 0.0;
                CircuitWarning.Warning(this, $"Xjbvs reset to {model.BSIM4xjbvs}.");
            }

            if (model.BSIM4bvs <= 0.0)
            {
                model.BSIM4bvs.Value = 0.0;
                CircuitWarning.Warning(this, $"BVS reset to {model.BSIM4bvs}.");
            }

            T0 = (model.TRatio - 1.0);
            model.BSIM4njtsstemp = model.BSIM4njts * (1.0 + model.BSIM4tnjts * T0);
            model.BSIM4njtsswstemp = model.BSIM4njtssw * (1.0 + model.BSIM4tnjtssw * T0);
            model.BSIM4njtsswgstemp = model.BSIM4njtsswg * (1.0 + model.BSIM4tnjtsswg * T0);
            model.BSIM4njtsdtemp = model.BSIM4njtsd * (1.0 + model.BSIM4tnjtsd * T0);
            model.BSIM4njtsswdtemp = model.BSIM4njtsswd * (1.0 + model.BSIM4tnjtsswd * T0);
            model.BSIM4njtsswgdtemp = model.BSIM4njtsswgd * (1.0 + model.BSIM4tnjtsswgd * T0);
        }
    }
}
