using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Temperature behavior for a <see cref="Components.Model"/>
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
        public double Fact1 { get; protected set; }
        public double Vtnom { get; protected set; }
        public double Egfet1 { get; protected set; }
        public double Pbfact1 { get; protected set; }
        public double OxideCapFactor { get; internal set; }
        [PropertyName("xd"), PropertyInfo("Depletion layer width")]
        public double CoeffDepLayWidth { get; internal set; }
        [PropertyName("alpha"), PropertyInfo("Alpha")]
        public double Alpha { get; internal set; }

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

            double kt1, arg1, fermis, wkfng, fermig, wkfngs, vfb;

            if (!mbp.NominalTemperature.Given)
            {
                mbp.NominalTemperature.Value = simulation.State.NominalTemperature;
            }
            Fact1 = mbp.NominalTemperature / Circuit.ReferenceTemperature;
            Vtnom = mbp.NominalTemperature * Circuit.KOverQ;
            kt1 = Circuit.Boltzmann * mbp.NominalTemperature;
            Egfet1 = 1.16 - (7.02e-4 * mbp.NominalTemperature * mbp.NominalTemperature) / (mbp.NominalTemperature + 1108);
            arg1 = -Egfet1 / (kt1 + kt1) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature + Circuit.ReferenceTemperature));
            Pbfact1 = -2 * Vtnom * (1.5 * Math.Log(Fact1) + Circuit.Charge * arg1);

            OxideCapFactor = 3.9 * 8.854214871e-12 / mbp.OxideThickness;
            if (!mbp.SurfaceMobility.Given)
                mbp.SurfaceMobility.Value = 600;
            if (!mbp.Transconductance.Given)
            {
                mbp.Transconductance.Value = mbp.SurfaceMobility * OxideCapFactor * 1e-4;
            }
            if (mbp.SubstrateDoping.Given)
            {
                if (mbp.SubstrateDoping * 1e6 /* (cm**3 / m**3) */ > 1.45e16)
                {
                    if (!mbp.Phi.Given)
                    {
                        mbp.Phi.Value = 2 * Vtnom * Math.Log(mbp.SubstrateDoping * 1e6 /* (cm *  * 3 / m *  * 3) */  / 1.45e16);
                        mbp.Phi.Value = Math.Max(.1, mbp.Phi);
                    }
                    fermis = mbp.Type * .5 * mbp.Phi;
                    wkfng = 3.2;
                    if (!mbp.GateType.Given)
                        mbp.GateType.Value = 1;
                    if (mbp.GateType != 0)
                    {
                        fermig = mbp.Type * mbp.GateType * .5 * Egfet1;
                        wkfng = 3.25 + .5 * Egfet1 - fermig;
                    }
                    wkfngs = wkfng - (3.25 + .5 * Egfet1 + fermis);
                    if (!mbp.Gamma.Given)
                    {
                        mbp.Gamma.Value = Math.Sqrt(2 * Transistor.EPSSIL * Circuit.Charge * mbp.SubstrateDoping * 1e6 /* (cm**3 / m**3) */) /
                            OxideCapFactor;
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
                    Alpha = (Transistor.EPSSIL + Transistor.EPSSIL) / (Circuit.Charge * mbp.SubstrateDoping * 1e6 /* (cm**3 / m**3) */);
                    CoeffDepLayWidth = Math.Sqrt(Alpha);
                }
                else
                {
                    mbp.SubstrateDoping.Value = 0;
                    throw new CircuitException($"{Name}: Nsub < Ni");
                }
            }
            /* now model parameter preprocessing */
            mbp.NarrowFactor = mbp.Delta * 0.5 * Math.PI * Transistor.EPSSIL / OxideCapFactor;
        }
    }
}
