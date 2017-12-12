using System;
using SpiceSharp.Parameters;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="BJT"/>
    /// </summary>
    public class BJTTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BJTModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("temp"), SpiceInfo("Instance temperature")]
        public double BJT_TEMP
        {
            get => BJTtemp - Circuit.CONSTCtoK;
            set => BJTtemp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter BJTtemp { get; } = new Parameter(300.15);
        public double BJTtSatCur { get; protected set; }
        public double BJTtBetaF { get; protected set; }
        public double BJTtBetaR { get; protected set; }
        public double BJTtBEleakCur { get; protected set; }
        public double BJTtBCleakCur { get; protected set; }
        public double BJTtBEcap { get; protected set; }
        public double BJTtBEpot { get; protected set; }
        public double BJTtBCcap { get; protected set; }
        public double BJTtBCpot { get; protected set; }
        public double BJTtDepCap { get; protected set; }
        public double BJTtf1 { get; protected set; }
        public double BJTtf4 { get; protected set; }
        public double BJTtf5 { get; protected set; }
        public double BJTtVcrit { get; protected set; }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            var bjt = component as BJT;
            modeltemp = GetBehavior<BJTModelTemperatureBehavior>(bjt.Model);
            return true;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double vt, fact2, egfet, arg, pbfact, ratlog, ratio1, factlog, factor, bfactor, pbo, gmaold, gmanew;

            if (!BJTtemp.Given)
                BJTtemp.Value = ckt.State.Temperature;
            vt = BJTtemp * Circuit.CONSTKoverQ;
            fact2 = BJTtemp / Circuit.CONSTRefTemp;
            egfet = 1.16 - (7.02e-4 * BJTtemp * BJTtemp) / (BJTtemp + 1108);
            arg = -egfet / (2 * Circuit.CONSTBoltz * BJTtemp) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp +
                 Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            ratlog = Math.Log(BJTtemp / modeltemp.BJTtnom);
            ratio1 = BJTtemp / modeltemp.BJTtnom - 1;
            factlog = ratio1 * modeltemp.BJTenergyGap / vt + modeltemp.BJTtempExpIS * ratlog;
            factor = Math.Exp(factlog);
            BJTtSatCur = modeltemp.BJTsatCur * factor;
            bfactor = Math.Exp(ratlog * modeltemp.BJTbetaExp);
            BJTtBetaF = modeltemp.BJTbetaF * bfactor;
            BJTtBetaR = modeltemp.BJTbetaR * bfactor;
            BJTtBEleakCur = modeltemp.BJTleakBEcurrent * Math.Exp(factlog / modeltemp.BJTleakBEemissionCoeff) / bfactor;
            BJTtBCleakCur = modeltemp.BJTleakBCcurrent * Math.Exp(factlog / modeltemp.BJTleakBCemissionCoeff) / bfactor;

            pbo = (modeltemp.BJTpotentialBE - pbfact) / modeltemp.fact1;
            gmaold = (modeltemp.BJTpotentialBE - pbo) / pbo;
            BJTtBEcap = modeltemp.BJTdepletionCapBE / (1 + modeltemp.BJTjunctionExpBE * (4e-4 * (modeltemp.BJTtnom - Circuit.CONSTRefTemp) - gmaold));
            BJTtBEpot = fact2 * pbo + pbfact;
            gmanew = (BJTtBEpot - pbo) / pbo;
            BJTtBEcap *= 1 + modeltemp.BJTjunctionExpBE * (4e-4 * (BJTtemp - Circuit.CONSTRefTemp) - gmanew);

            pbo = (modeltemp.BJTpotentialBC - pbfact) / modeltemp.fact1;
            gmaold = (modeltemp.BJTpotentialBC - pbo) / pbo;
            BJTtBCcap = modeltemp.BJTdepletionCapBC / (1 + modeltemp.BJTjunctionExpBC * (4e-4 * (modeltemp.BJTtnom - Circuit.CONSTRefTemp) - gmaold));
            BJTtBCpot = fact2 * pbo + pbfact;
            gmanew = (BJTtBCpot - pbo) / pbo;
            BJTtBCcap *= 1 + modeltemp.BJTjunctionExpBC * (4e-4 * (BJTtemp - Circuit.CONSTRefTemp) - gmanew);

            BJTtDepCap = modeltemp.BJTdepletionCapCoeff * BJTtBEpot;
            BJTtf1 = BJTtBEpot * (1 - Math.Exp((1 - modeltemp.BJTjunctionExpBE) * modeltemp.xfc)) / (1 - modeltemp.BJTjunctionExpBE);
            BJTtf4 = modeltemp.BJTdepletionCapCoeff * BJTtBCpot;
            BJTtf5 = BJTtBCpot * (1 - Math.Exp((1 - modeltemp.BJTjunctionExpBC) * modeltemp.xfc)) / (1 - modeltemp.BJTjunctionExpBC);
            BJTtVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * modeltemp.BJTsatCur));
        }
    }
}
