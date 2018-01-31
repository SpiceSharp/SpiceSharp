using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="BipolarJunctionTransistor"/>
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
        public double TempSaturationCurrent { get; protected set; }
        public double TempBetaForward { get; protected set; }
        public double TempBetaReverse { get; protected set; }
        public double TempBELeakageCurrent { get; protected set; }
        public double TempBCLeakageCurrent { get; protected set; }
        public double TempBECap { get; protected set; }
        public double TempBEPotential { get; protected set; }
        public double TempBCCap { get; protected set; }
        public double TempBCPotential { get; protected set; }
        public double TempDepletionCap { get; protected set; }
        public double TempFactor1 { get; protected set; }
        public double TempFactor4 { get; protected set; }
        public double TempFactor5 { get; protected set; }
        public double TempVCrit { get; protected set; }

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
        /// <param name="simulation">Base simulation</param>
        public override void Temperature(BaseSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double vt, fact2, egfet, arg, pbfact, ratlog, ratio1, factlog, factor, bfactor, pbo, gmaold, gmanew;

            if (!bp.Temperature.Given)
                bp.Temperature.Value = simulation.State.Temperature;
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
            TempSaturationCurrent = mbp.SatCur * factor;
            bfactor = Math.Exp(ratlog * mbp.BetaExponent);
            TempBetaForward = mbp.BetaF * bfactor;
            TempBetaReverse = mbp.BetaR * bfactor;
            TempBELeakageCurrent = mbp.LeakBECurrent * Math.Exp(factlog / mbp.LeakBEEmissionCoefficient) / bfactor;
            TempBCLeakageCurrent = mbp.LeakBCCurrent * Math.Exp(factlog / mbp.LeakBCEmissionCoefficient) / bfactor;

            pbo = (mbp.PotentialBE - pbfact) / modeltemp.Fact1;
            gmaold = (mbp.PotentialBE - pbo) / pbo;
            TempBECap = mbp.DepletionCapBE / (1 + mbp.JunctionExpBE * (4e-4 * (mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TempBEPotential = fact2 * pbo + pbfact;
            gmanew = (TempBEPotential - pbo) / pbo;
            TempBECap *= 1 + mbp.JunctionExpBE * (4e-4 * (bp.Temperature - Circuit.ReferenceTemperature) - gmanew);

            pbo = (mbp.PotentialBC - pbfact) / modeltemp.Fact1;
            gmaold = (mbp.PotentialBC - pbo) / pbo;
            TempBCCap = mbp.DepletionCapBC / (1 + mbp.JunctionExpBC * (4e-4 * (mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TempBCPotential = fact2 * pbo + pbfact;
            gmanew = (TempBCPotential - pbo) / pbo;
            TempBCCap *= 1 + mbp.JunctionExpBC * (4e-4 * (bp.Temperature - Circuit.ReferenceTemperature) - gmanew);

            TempDepletionCap = mbp.DepletionCapCoefficient * TempBEPotential;
            TempFactor1 = TempBEPotential * (1 - Math.Exp((1 - mbp.JunctionExpBE) * modeltemp.Xfc)) / (1 - mbp.JunctionExpBE);
            TempFactor4 = mbp.DepletionCapCoefficient * TempBCPotential;
            TempFactor5 = TempBCPotential * (1 - Math.Exp((1 - mbp.JunctionExpBC) * modeltemp.Xfc)) / (1 - mbp.JunctionExpBC);
            TempVCrit = vt * Math.Log(vt / (Circuit.Root2 * mbp.SatCur));
        }
    }
}
