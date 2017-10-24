using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="BSIM3v24Model"/>
    /// </summary>
    public class BSIM3v24ModelTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {

            var model = ComponentTyped<BSIM3v24Model>();
            double Temp, Tnom, Eg0, Eg, delTemp, T0, T1;

            Temp = ckt.State.Temperature;
            if (model.BSIM3bulkJctPotential < 0.1)
            {
                model.BSIM3bulkJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pb is less than 0.1. Pb is set to 0.1");
            }
            if (model.BSIM3sidewallJctPotential < 0.1)
            {
                model.BSIM3sidewallJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbsw is less than 0.1. Pbsw is set to 0.1");
            }
            if (model.BSIM3GatesidewallJctPotential < 0.1)
            {
                model.BSIM3GatesidewallJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbswg is less than 0.1. Pbswg is set to 0.1");
            }

            Tnom = model.BSIM3tnom;
            model.TRatio = Temp / Tnom;

            model.BSIM3vcrit = Circuit.CONSTvt0 * Math.Log(Circuit.CONSTvt0 / (Circuit.CONSTroot2 * 1.0e-14));
            model.BSIM3factor1 = Math.Sqrt(Transistor.EPSSI / Transistor.EPSOX * model.BSIM3tox);

            model.Vtm0 = Transistor.KboQ * Tnom;
            Eg0 = 1.16 - 7.02e-4 * Tnom * Tnom / (Tnom + 1108.0);
            model.ni = 1.45e10 * (Tnom / 300.15) * Math.Sqrt(Tnom / 300.15) * Math.Exp(21.5565981 - Eg0 / (2.0 * model.Vtm0));

            model.BSIM3vtm = Transistor.KboQ * Temp;
            Eg = 1.16 - 7.02e-4 * Temp * Temp / (Temp + 1108.0);
            if (Temp != Tnom)
            {
                T0 = Eg0 / model.Vtm0 - Eg / model.BSIM3vtm + model.BSIM3jctTempExponent * Math.Log(Temp / Tnom);
                T1 = Math.Exp(T0 / model.BSIM3jctEmissionCoeff);
                model.BSIM3jctTempSatCurDensity = model.BSIM3jctSatCurDensity * T1;
                model.BSIM3jctSidewallTempSatCurDensity = model.BSIM3jctSidewallSatCurDensity * T1;
            }
            else
            {
                model.BSIM3jctTempSatCurDensity = model.BSIM3jctSatCurDensity;
                model.BSIM3jctSidewallTempSatCurDensity = model.BSIM3jctSidewallSatCurDensity;
            }

            if (model.BSIM3jctTempSatCurDensity < 0.0)
                model.BSIM3jctTempSatCurDensity = 0.0;
            if (model.BSIM3jctSidewallTempSatCurDensity < 0.0)
                model.BSIM3jctSidewallTempSatCurDensity = 0.0;

            /* Temperature dependence of D / B and S / B diode capacitance begins */
            delTemp = ckt.State.Temperature - model.BSIM3tnom;
            T0 = model.BSIM3tcj * delTemp;
            if (T0 >= -1.0)
            {
                model.BSIM3unitAreaTempJctCap = model.BSIM3unitAreaJctCap * (1.0 + T0);
            }
            else if (model.BSIM3unitAreaJctCap > 0.0)
            {
                model.BSIM3unitAreaTempJctCap = 0.0;
                CircuitWarning.Warning(this, "Temperature effect has caused cj to be negative. Cj is clamped to zero");
            }
            T0 = model.BSIM3tcjsw * delTemp;
            if (T0 >= -1.0)
            {
                model.BSIM3unitLengthSidewallTempJctCap = model.BSIM3unitLengthSidewallJctCap * (1.0 + T0);
            }
            else if (model.BSIM3unitLengthSidewallJctCap > 0.0)
            {
                model.BSIM3unitLengthSidewallTempJctCap = 0.0;
                CircuitWarning.Warning(this, "Temperature effect has caused cjsw to be negative. Cjsw is clamped to zero");
            }
            T0 = model.BSIM3tcjswg * delTemp;
            if (T0 >= -1.0)
            {
                model.BSIM3unitLengthGateSidewallTempJctCap = model.BSIM3unitLengthGateSidewallJctCap * (1.0 + T0);
            }
            else if (model.BSIM3unitLengthGateSidewallJctCap > 0.0)
            {
                model.BSIM3unitLengthGateSidewallTempJctCap = 0.0;
                CircuitWarning.Warning(this, "Temperature effect has caused cjswg to be negative. Cjswg is clamped to zero");
            }

            model.BSIM3PhiB = model.BSIM3bulkJctPotential - model.BSIM3tpb * delTemp;
            if (model.BSIM3PhiB < 0.01)
            {
                model.BSIM3PhiB = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pb to be less than 0.01. Pb is clamped to 0.01");
            }
            model.BSIM3PhiBSW = model.BSIM3sidewallJctPotential - model.BSIM3tpbsw * delTemp;
            if (model.BSIM3PhiBSW <= 0.01)
            {
                model.BSIM3PhiBSW = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbsw to be less than 0.01. Pbsw is clamped to 0.01");
            }
            model.BSIM3PhiBSWG = model.BSIM3GatesidewallJctPotential - model.BSIM3tpbswg * delTemp;
            if (model.BSIM3PhiBSWG <= 0.01)
            {
                model.BSIM3PhiBSWG = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbswg to be less than 0.01. Pbswg is clamped to 0.01");
            }
        }
    }
}
