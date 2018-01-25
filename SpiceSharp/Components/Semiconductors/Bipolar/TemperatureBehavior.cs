using System;
using SpiceSharp.Attributes;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Bipolar;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.Bipolar
{
    /// <summary>
    /// Temperature behavior for a <see cref="Components.BJT"/>
    /// </summary>
    public class TemperatureBehavior : Behaviors.TemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        ModelBaseParameters mbp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("temp"), PropertyInfo("Instance temperature")]
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
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            mbp = provider.GetParameters<ModelBaseParameters>(1);

            // Get behaviors
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Temperature(BaseSimulation sim)
        {
            double vt, fact2, egfet, arg, pbfact, ratlog, ratio1, factlog, factor, bfactor, pbo, gmaold, gmanew;

            if (!BJTtemp.Given)
                BJTtemp.Value = sim.State.Temperature;
            vt = BJTtemp * Circuit.CONSTKoverQ;
            fact2 = BJTtemp / Circuit.CONSTRefTemp;
            egfet = 1.16 - (7.02e-4 * BJTtemp * BJTtemp) / (BJTtemp + 1108);
            arg = -egfet / (2 * Circuit.CONSTBoltz * BJTtemp) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp +
                 Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            ratlog = Math.Log(BJTtemp / mbp.BJTtnom);
            ratio1 = BJTtemp / mbp.BJTtnom - 1;
            factlog = ratio1 * mbp.BJTenergyGap / vt + mbp.BJTtempExpIS * ratlog;
            factor = Math.Exp(factlog);
            BJTtSatCur = mbp.BJTsatCur * factor;
            bfactor = Math.Exp(ratlog * mbp.BJTbetaExp);
            BJTtBetaF = mbp.BJTbetaF * bfactor;
            BJTtBetaR = mbp.BJTbetaR * bfactor;
            BJTtBEleakCur = mbp.BJTleakBEcurrent * Math.Exp(factlog / mbp.BJTleakBEemissionCoeff) / bfactor;
            BJTtBCleakCur = mbp.BJTleakBCcurrent * Math.Exp(factlog / mbp.BJTleakBCemissionCoeff) / bfactor;

            pbo = (mbp.BJTpotentialBE - pbfact) / modeltemp.fact1;
            gmaold = (mbp.BJTpotentialBE - pbo) / pbo;
            BJTtBEcap = mbp.BJTdepletionCapBE / (1 + mbp.BJTjunctionExpBE * (4e-4 * (mbp.BJTtnom - Circuit.CONSTRefTemp) - gmaold));
            BJTtBEpot = fact2 * pbo + pbfact;
            gmanew = (BJTtBEpot - pbo) / pbo;
            BJTtBEcap *= 1 + mbp.BJTjunctionExpBE * (4e-4 * (BJTtemp - Circuit.CONSTRefTemp) - gmanew);

            pbo = (mbp.BJTpotentialBC - pbfact) / modeltemp.fact1;
            gmaold = (mbp.BJTpotentialBC - pbo) / pbo;
            BJTtBCcap = mbp.BJTdepletionCapBC / (1 + mbp.BJTjunctionExpBC * (4e-4 * (mbp.BJTtnom - Circuit.CONSTRefTemp) - gmaold));
            BJTtBCpot = fact2 * pbo + pbfact;
            gmanew = (BJTtBCpot - pbo) / pbo;
            BJTtBCcap *= 1 + mbp.BJTjunctionExpBC * (4e-4 * (BJTtemp - Circuit.CONSTRefTemp) - gmanew);

            BJTtDepCap = mbp.BJTdepletionCapCoeff * BJTtBEpot;
            BJTtf1 = BJTtBEpot * (1 - Math.Exp((1 - mbp.BJTjunctionExpBE) * modeltemp.xfc)) / (1 - mbp.BJTjunctionExpBE);
            BJTtf4 = mbp.BJTdepletionCapCoeff * BJTtBCpot;
            BJTtf5 = BJTtBCpot * (1 - Math.Exp((1 - mbp.BJTjunctionExpBC) * modeltemp.xfc)) / (1 - mbp.BJTjunctionExpBC);
            BJTtVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * mbp.BJTsatCur));
        }
    }
}
