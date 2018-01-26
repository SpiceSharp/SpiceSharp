using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="BJT"/>
    /// </summary>
    public class TemperatureBehavior : Behaviors.TemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double TSatCur { get; protected set; }
        public double TBetaF { get; protected set; }
        public double TBetaR { get; protected set; }
        public double TBEleakCur { get; protected set; }
        public double TBCleakCur { get; protected set; }
        public double TBEcap { get; protected set; }
        public double TBEpot { get; protected set; }
        public double TBCcap { get; protected set; }
        public double TBCpot { get; protected set; }
        public double TDepCap { get; protected set; }
        public double Tf1 { get; protected set; }
        public double Tf4 { get; protected set; }
        public double Tf5 { get; protected set; }
        public double TVcrit { get; protected set; }

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
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
            mbp = provider.GetParameterSet<ModelBaseParameters>(1);

            // Get behaviors
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Temperature(BaseSimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            double vt, fact2, egfet, arg, pbfact, ratlog, ratio1, factlog, factor, bfactor, pbo, gmaold, gmanew;

            if (!bp.Temperature.Given)
                bp.Temperature.Value = sim.State.Temperature;
            vt = bp.Temperature * Circuit.KOverQ;
            fact2 = bp.Temperature / Circuit.ReferenceTemperature;
            egfet = 1.16 - (7.02e-4 * bp.Temperature * bp.Temperature) / (bp.Temperature + 1108);
            arg = -egfet / (2 * Circuit.Boltzmann * bp.Temperature) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature +
                 Circuit.ReferenceTemperature));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.Charge * arg);

            ratlog = Math.Log(bp.Temperature / mbp.NominalTemperature);
            ratio1 = bp.Temperature / mbp.NominalTemperature - 1;
            factlog = ratio1 * mbp.EnergyGap / vt + mbp.TempExpIS * ratlog;
            factor = Math.Exp(factlog);
            TSatCur = mbp.SatCur * factor;
            bfactor = Math.Exp(ratlog * mbp.BetaExp);
            TBetaF = mbp.BetaF * bfactor;
            TBetaR = mbp.BetaR * bfactor;
            TBEleakCur = mbp.LeakBEcurrent * Math.Exp(factlog / mbp.LeakBEemissionCoeff) / bfactor;
            TBCleakCur = mbp.LeakBCcurrent * Math.Exp(factlog / mbp.LeakBCemissionCoeff) / bfactor;

            pbo = (mbp.PotentialBE - pbfact) / modeltemp.Fact1;
            gmaold = (mbp.PotentialBE - pbo) / pbo;
            TBEcap = mbp.DepletionCapBE / (1 + mbp.JunctionExpBE * (4e-4 * (mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TBEpot = fact2 * pbo + pbfact;
            gmanew = (TBEpot - pbo) / pbo;
            TBEcap *= 1 + mbp.JunctionExpBE * (4e-4 * (bp.Temperature - Circuit.ReferenceTemperature) - gmanew);

            pbo = (mbp.PotentialBC - pbfact) / modeltemp.Fact1;
            gmaold = (mbp.PotentialBC - pbo) / pbo;
            TBCcap = mbp.DepletionCapBC / (1 + mbp.JunctionExpBC * (4e-4 * (mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TBCpot = fact2 * pbo + pbfact;
            gmanew = (TBCpot - pbo) / pbo;
            TBCcap *= 1 + mbp.JunctionExpBC * (4e-4 * (bp.Temperature - Circuit.ReferenceTemperature) - gmanew);

            TDepCap = mbp.DepletionCapCoeff * TBEpot;
            Tf1 = TBEpot * (1 - Math.Exp((1 - mbp.JunctionExpBE) * modeltemp.Xfc)) / (1 - mbp.JunctionExpBE);
            Tf4 = mbp.DepletionCapCoeff * TBCpot;
            Tf5 = TBCpot * (1 - Math.Exp((1 - mbp.JunctionExpBC) * modeltemp.Xfc)) / (1 - mbp.JunctionExpBC);
            TVcrit = vt * Math.Log(vt / (Circuit.Root2 * mbp.SatCur));
        }
    }
}
