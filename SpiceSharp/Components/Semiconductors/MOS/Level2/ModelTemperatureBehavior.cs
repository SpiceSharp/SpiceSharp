using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
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
        /// Shared parameters
        /// </summary>
        public double fact1 { get; protected set; }
        public double vtnom { get; protected set; }
        public double egfet1 { get; protected set; }
        public double pbfact1 { get; protected set; }
        public double OxideCapFactor { get; internal set; }
        public double Xd { get; internal set; }

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

            /* now model parameter preprocessing */
            if (!mbp.NominalTemperature.Given)
                mbp.NominalTemperature.Value = simulation.State.NominalTemperature;
            fact1 = mbp.NominalTemperature / Circuit.ReferenceTemperature;
            vtnom = mbp.NominalTemperature * Circuit.KOverQ;
            kt1 = Circuit.Boltzmann * mbp.NominalTemperature;
            egfet1 = 1.16 - (7.02e-4 * mbp.NominalTemperature * mbp.NominalTemperature) / (mbp.NominalTemperature + 1108);
            arg1 = -egfet1 / (kt1 + kt1) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature + Circuit.ReferenceTemperature));
            pbfact1 = -2 * vtnom * (1.5 * Math.Log(fact1) + Circuit.Charge * arg1);

            if (!mbp.OxideThickness.Given)
            {
                mbp.OxideThickness.Value = 1e-7;
            }
            OxideCapFactor = 3.9 * 8.854214871e-12 / mbp.OxideThickness;

            if (!mbp.SurfaceMobility.Given)
                mbp.SurfaceMobility.Value = 600;
            if (!mbp.Transconductance.Given)
            {
                mbp.Transconductance.Value = mbp.SurfaceMobility * 1e-4 * OxideCapFactor;
            }
            if (mbp.SubstrateDoping.Given)
            {
                if (mbp.SubstrateDoping * 1e6 > 1.45e16)
                {
                    if (!mbp.Phi.Given)
                    {
                        mbp.Phi.Value = 2 * vtnom * Math.Log(mbp.SubstrateDoping * 1e6 / 1.45e16);
                        mbp.Phi.Value = Math.Max(.1, mbp.Phi);
                    }
                    fermis = mbp.Type * .5 * mbp.Phi;
                    wkfng = 3.2;
                    if (!mbp.GateType.Given)
                        mbp.GateType.Value = 1;
                    if (mbp.GateType != 0)
                    {
                        fermig = mbp.Type * mbp.GateType * .5 * egfet1;
                        wkfng = 3.25 + .5 * egfet1 - fermig;
                    }
                    wkfngs = wkfng - (3.25 + .5 * egfet1 + fermis);
                    if (!mbp.Gamma.Given)
                    {
                        mbp.Gamma.Value = Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Circuit.Charge * mbp.SubstrateDoping * 1e6) / OxideCapFactor;
                    }
                    if (!mbp.Vt0.Given)
                    {
                        if (!mbp.SurfaceStateDensity.Given)
                            mbp.SurfaceStateDensity.Value = 0;
                        vfb = wkfngs - mbp.SurfaceStateDensity * 1e4 * Circuit.Charge / OxideCapFactor;
                        mbp.Vt0.Value = vfb + mbp.Type * (mbp.Gamma * Math.Sqrt(mbp.Phi) + mbp.Phi);
                    }
                    else
                    {
                        vfb = mbp.Vt0 - mbp.Type * (mbp.Gamma * Math.Sqrt(mbp.Phi) + mbp.Phi);
                    }
                    Xd = Math.Sqrt((Transistor.EPSSIL + Transistor.EPSSIL) / (Circuit.Charge * mbp.SubstrateDoping * 1e6));
                }
                else
                {
                    mbp.SubstrateDoping.Value = 0;
                    throw new CircuitException($"{Name}: Nsub < Ni");
                }
            }
            if (!mbp.BulkCapFactor.Given)
            {
                mbp.BulkCapFactor.Value = Math.Sqrt(Transistor.EPSSIL * Circuit.Charge * mbp.SubstrateDoping * 1e6 /* cm**3/m**3 */  / (2 *
                    mbp.BulkJctPotential));
            }
        }
    }
}
