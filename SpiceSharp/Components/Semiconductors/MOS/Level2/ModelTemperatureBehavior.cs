using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.Transistors;
using SpiceSharp.Components.Mosfet.Level2;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.Mosfet.Level2
{
    /// <summary>
    /// Temperature behavior for a <see cref="Components.MOS2Model"/>
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
        public double MOS2oxideCapFactor { get; internal set; }
        public double MOS2xd { get; internal set; }

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
            // Get parameters
            mbp = provider.GetParameterSet<ModelBaseParameters>(0);
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Temperature(BaseSimulation sim)
        {
            double kt1, arg1, fermis, wkfng, fermig, wkfngs, vfb = 0.0;

            /* now model parameter preprocessing */
            if (!mbp.MOS2tnom.Given)
                mbp.MOS2tnom.Value = sim.State.NominalTemperature;
            fact1 = mbp.MOS2tnom / Circuit.ReferenceTemperature;
            vtnom = mbp.MOS2tnom * Circuit.KOverQ;
            kt1 = Circuit.Boltzmann * mbp.MOS2tnom;
            egfet1 = 1.16 - (7.02e-4 * mbp.MOS2tnom * mbp.MOS2tnom) / (mbp.MOS2tnom + 1108);
            arg1 = -egfet1 / (kt1 + kt1) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature + Circuit.ReferenceTemperature));
            pbfact1 = -2 * vtnom * (1.5 * Math.Log(fact1) + Circuit.Charge * arg1);

            if (!mbp.MOS2oxideThickness.Given)
            {
                mbp.MOS2oxideThickness.Value = 1e-7;
            }
            MOS2oxideCapFactor = 3.9 * 8.854214871e-12 / mbp.MOS2oxideThickness;

            if (!mbp.MOS2surfaceMobility.Given)
                mbp.MOS2surfaceMobility.Value = 600;
            if (!mbp.MOS2transconductance.Given)
            {
                mbp.MOS2transconductance.Value = mbp.MOS2surfaceMobility * 1e-4 * MOS2oxideCapFactor;
            }
            if (mbp.MOS2substrateDoping.Given)
            {
                if (mbp.MOS2substrateDoping * 1e6 > 1.45e16)
                {
                    if (!mbp.MOS2phi.Given)
                    {
                        mbp.MOS2phi.Value = 2 * vtnom * Math.Log(mbp.MOS2substrateDoping * 1e6 / 1.45e16);
                        mbp.MOS2phi.Value = Math.Max(.1, mbp.MOS2phi);
                    }
                    fermis = mbp.MOS2type * .5 * mbp.MOS2phi;
                    wkfng = 3.2;
                    if (!mbp.MOS2gateType.Given)
                        mbp.MOS2gateType.Value = 1;
                    if (mbp.MOS2gateType != 0)
                    {
                        fermig = mbp.MOS2type * mbp.MOS2gateType * .5 * egfet1;
                        wkfng = 3.25 + .5 * egfet1 - fermig;
                    }
                    wkfngs = wkfng - (3.25 + .5 * egfet1 + fermis);
                    if (!mbp.MOS2gamma.Given)
                    {
                        mbp.MOS2gamma.Value = Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Circuit.Charge * mbp.MOS2substrateDoping * 1e6) / MOS2oxideCapFactor;
                    }
                    if (!mbp.MOS2vt0.Given)
                    {
                        if (!mbp.MOS2surfaceStateDensity.Given)
                            mbp.MOS2surfaceStateDensity.Value = 0;
                        vfb = wkfngs - mbp.MOS2surfaceStateDensity * 1e4 * Circuit.Charge / MOS2oxideCapFactor;
                        mbp.MOS2vt0.Value = vfb + mbp.MOS2type * (mbp.MOS2gamma * Math.Sqrt(mbp.MOS2phi) + mbp.MOS2phi);
                    }
                    else
                    {
                        vfb = mbp.MOS2vt0 - mbp.MOS2type * (mbp.MOS2gamma * Math.Sqrt(mbp.MOS2phi) + mbp.MOS2phi);
                    }
                    MOS2xd = Math.Sqrt((Transistor.EPSSIL + Transistor.EPSSIL) / (Circuit.Charge * mbp.MOS2substrateDoping * 1e6));
                }
                else
                {
                    mbp.MOS2substrateDoping.Value = 0;
                    throw new CircuitException($"{Name}: Nsub < Ni");
                }
            }
            if (!mbp.MOS2bulkCapFactor.Given)
            {
                mbp.MOS2bulkCapFactor.Value = Math.Sqrt(Transistor.EPSSIL * Circuit.Charge * mbp.MOS2substrateDoping * 1e6 /* cm**3/m**3 */  / (2 *
                    mbp.MOS2bulkJctPotential));
            }
        }
    }
}
