using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.Transistors;
using SpiceSharp.Attributes;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Mosfet.Level3;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.Mosfet.Level3
{
    /// <summary>
    /// Temperature behavior for a <see cref="Components.MOS3Model"/>
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
        public double MOS3oxideCapFactor { get; internal set; }
        [PropertyName("xd"), PropertyInfo("Depletion layer width")]
        public double MOS3coeffDepLayWidth { get; internal set; }
        [PropertyName("alpha"), PropertyInfo("Alpha")]
        public double MOS3alpha { get; internal set; }

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
        /// <param name="sim">Base simulation</param>
        public override void Temperature(BaseSimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            double kt1, arg1, fermis, wkfng, fermig, wkfngs, vfb;

            if (!mbp.MOS3tnom.Given)
            {
                mbp.MOS3tnom.Value = sim.State.NominalTemperature;
            }
            fact1 = mbp.MOS3tnom / Circuit.ReferenceTemperature;
            vtnom = mbp.MOS3tnom * Circuit.KOverQ;
            kt1 = Circuit.Boltzmann * mbp.MOS3tnom;
            egfet1 = 1.16 - (7.02e-4 * mbp.MOS3tnom * mbp.MOS3tnom) / (mbp.MOS3tnom + 1108);
            arg1 = -egfet1 / (kt1 + kt1) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature + Circuit.ReferenceTemperature));
            pbfact1 = -2 * vtnom * (1.5 * Math.Log(fact1) + Circuit.Charge * arg1);

            MOS3oxideCapFactor = 3.9 * 8.854214871e-12 / mbp.MOS3oxideThickness;
            if (!mbp.MOS3surfaceMobility.Given)
                mbp.MOS3surfaceMobility.Value = 600;
            if (!mbp.MOS3transconductance.Given)
            {
                mbp.MOS3transconductance.Value = mbp.MOS3surfaceMobility * MOS3oxideCapFactor * 1e-4;
            }
            if (mbp.MOS3substrateDoping.Given)
            {
                if (mbp.MOS3substrateDoping * 1e6 /* (cm**3 / m**3) */ > 1.45e16)
                {
                    if (!mbp.MOS3phi.Given)
                    {
                        mbp.MOS3phi.Value = 2 * vtnom * Math.Log(mbp.MOS3substrateDoping * 1e6 /* (cm *  * 3 / m *  * 3) */  / 1.45e16);
                        mbp.MOS3phi.Value = Math.Max(.1, mbp.MOS3phi);
                    }
                    fermis = mbp.MOS3type * .5 * mbp.MOS3phi;
                    wkfng = 3.2;
                    if (!mbp.MOS3gateType.Given)
                        mbp.MOS3gateType.Value = 1;
                    if (mbp.MOS3gateType != 0)
                    {
                        fermig = mbp.MOS3type * mbp.MOS3gateType * .5 * egfet1;
                        wkfng = 3.25 + .5 * egfet1 - fermig;
                    }
                    wkfngs = wkfng - (3.25 + .5 * egfet1 + fermis);
                    if (!mbp.MOS3gamma.Given)
                    {
                        mbp.MOS3gamma.Value = Math.Sqrt(2 * Transistor.EPSSIL * Circuit.Charge * mbp.MOS3substrateDoping * 1e6 /* (cm**3 / m**3) */) /
                            MOS3oxideCapFactor;
                    }
                    if (!mbp.MOS3vt0.Given)
                    {
                        if (!mbp.MOS3surfaceStateDensity.Given)
                            mbp.MOS3surfaceStateDensity.Value = 0;
                        vfb = wkfngs - mbp.MOS3surfaceStateDensity * 1e4 * Circuit.Charge / MOS3oxideCapFactor;
                        mbp.MOS3vt0.Value = vfb + mbp.MOS3type * (mbp.MOS3gamma * Math.Sqrt(mbp.MOS3phi) + mbp.MOS3phi);
                    }
                    else
                    {
                        vfb = mbp.MOS3vt0 - mbp.MOS3type * (mbp.MOS3gamma * Math.Sqrt(mbp.MOS3phi) + mbp.MOS3phi);
                    }
                    MOS3alpha = (Transistor.EPSSIL + Transistor.EPSSIL) / (Circuit.Charge * mbp.MOS3substrateDoping * 1e6 /* (cm**3 / m**3) */);
                    MOS3coeffDepLayWidth = Math.Sqrt(MOS3alpha);
                }
                else
                {
                    mbp.MOS3substrateDoping.Value = 0;
                    throw new CircuitException($"{Name}: Nsub < Ni");
                }
            }
            /* now model parameter preprocessing */
            mbp.MOS3narrowFactor = mbp.MOS3delta * 0.5 * Math.PI * Transistor.EPSSIL / MOS3oxideCapFactor;
        }
    }
}
