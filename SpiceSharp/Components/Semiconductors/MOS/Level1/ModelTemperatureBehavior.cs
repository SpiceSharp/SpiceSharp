using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Temperature behavior for a <see cref="Model"/>
    /// </summary>
    public class ModelTemperatureBehavior : Behaviors.TemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        ModelBaseParameters mbp;

        /// <summary>
        /// Extra variables
        /// </summary>
        public double Fact1 { get; protected set; }
        public double VtNominal { get; protected set; }
        public double EgFet1 { get; protected set; }
        public double PbFactor1 { get; protected set; }
        public double OxideCapFactor { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public ModelTemperatureBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            mbp = provider.GetParameterSet<ModelBaseParameters>(0);
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Temperature(BaseSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double kt1, arg1, fermis, wkfng, fermig, wkfngs, vfb = 0.0;

            /* perform model defaulting */
            if (!mbp.NominalTemperature.Given)
                mbp.NominalTemperature.Value = simulation.State.NominalTemperature;

            Fact1 = mbp.NominalTemperature / Circuit.ReferenceTemperature;
            VtNominal = mbp.NominalTemperature * Circuit.KOverQ;
            kt1 = Circuit.Boltzmann * mbp.NominalTemperature;
            EgFet1 = 1.16 - (7.02e-4 * mbp.NominalTemperature * mbp.NominalTemperature) / (mbp.NominalTemperature + 1108);
            arg1 = -EgFet1 / (kt1 + kt1) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature + Circuit.ReferenceTemperature));
            PbFactor1 = -2 * VtNominal * (1.5 * Math.Log(Fact1) + Circuit.Charge * arg1);

            /* now model parameter preprocessing */

            if (!mbp.OxideThickness.Given || mbp.OxideThickness.Value == 0)
            {
                OxideCapFactor = 0;
            }
            else
            {
                OxideCapFactor = 3.9 * 8.854214871e-12 / mbp.OxideThickness;
                if (!mbp.Transconductance.Given)
                {
                    if (!mbp.SurfaceMobility.Given)
                    {
                        mbp.SurfaceMobility.Value = 600;
                    }
                    mbp.Transconductance.Value = mbp.SurfaceMobility * OxideCapFactor * 1e-4;
                }
                if (mbp.SubstrateDoping.Given)
                {
                    if (mbp.SubstrateDoping * 1e6 > 1.45e16)
                    {
                        if (!mbp.Phi.Given)
                        {
                            mbp.Phi.Value = 2 * VtNominal * Math.Log(mbp.SubstrateDoping * 1e6 / 1.45e16);
                            mbp.Phi.Value = Math.Max(.1, mbp.Phi);
                        }
                        fermis = mbp.MosfetType * .5 * mbp.Phi;
                        wkfng = 3.2;
                        if (!mbp.GateType.Given)
                            mbp.GateType.Value = 1;
                        if (mbp.GateType != 0)
                        {
                            fermig = mbp.MosfetType * mbp.GateType * .5 * EgFet1;
                            wkfng = 3.25 + .5 * EgFet1 - fermig;
                        }
                        wkfngs = wkfng - (3.25 + .5 * EgFet1 + fermis);
                        if (!mbp.Gamma.Given)
                        {
                            mbp.Gamma.Value = Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Circuit.Charge * mbp.SubstrateDoping * 1e6) / OxideCapFactor;
                        }
                        if (!mbp.VT0.Given)
                        {
                            if (!mbp.SurfaceStateDensity.Given)
                                mbp.SurfaceStateDensity.Value = 0;
                            vfb = wkfngs - mbp.SurfaceStateDensity * 1e4 * Circuit.Charge / OxideCapFactor;
                            mbp.VT0.Value = vfb + mbp.MosfetType * (mbp.Gamma * Math.Sqrt(mbp.Phi) + mbp.Phi);
                        }
                    }
                    else
                    {
                        mbp.SubstrateDoping.Value = 0;
                        throw new CircuitException("{0}: Nsub < Ni".FormatString(Name));
                    }
                }
            }
        }
    }
}
