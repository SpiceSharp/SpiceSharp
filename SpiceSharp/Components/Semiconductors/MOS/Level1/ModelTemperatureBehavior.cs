using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Mosfet.Level1;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.Mosfet.Level1
{
    /// <summary>
    /// Temperature behavior for a <see cref="Components.MOS1Model"/>
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
        public double fact1 { get; protected set; }
        public double vtnom { get; protected set; }
        public double egfet1 { get; protected set; }
        public double pbfact1 { get; protected set; }
        public double MOS1oxideCapFactor { get; internal set; }

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
            mbp = provider.GetParameters<ModelBaseParameters>();
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Temperature(BaseSimulation sim)
        {
            double kt1, arg1, fermis, wkfng, fermig, wkfngs, vfb = 0.0;

            /* perform model defaulting */
            if (!mbp.MOS1tnom.Given)
                mbp.MOS1tnom.Value = sim.State.NominalTemperature;

            fact1 = mbp.MOS1tnom / Circuit.ReferenceTemperature;
            vtnom = mbp.MOS1tnom * Circuit.KOverQ;
            kt1 = Circuit.Boltzmann * mbp.MOS1tnom;
            egfet1 = 1.16 - (7.02e-4 * mbp.MOS1tnom * mbp.MOS1tnom) / (mbp.MOS1tnom + 1108);
            arg1 = -egfet1 / (kt1 + kt1) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature + Circuit.ReferenceTemperature));
            pbfact1 = -2 * vtnom * (1.5 * Math.Log(fact1) + Circuit.Charge * arg1);

            /* now model parameter preprocessing */

            if (!mbp.MOS1oxideThickness.Given || mbp.MOS1oxideThickness.Value == 0)
            {
                MOS1oxideCapFactor = 0;
            }
            else
            {
                MOS1oxideCapFactor = 3.9 * 8.854214871e-12 / mbp.MOS1oxideThickness;
                if (!mbp.MOS1transconductance.Given)
                {
                    if (!mbp.MOS1surfaceMobility.Given)
                    {
                        mbp.MOS1surfaceMobility.Value = 600;
                    }
                    mbp.MOS1transconductance.Value = mbp.MOS1surfaceMobility * MOS1oxideCapFactor * 1e-4;
                }
                if (mbp.MOS1substrateDoping.Given)
                {
                    if (mbp.MOS1substrateDoping * 1e6 > 1.45e16)
                    {
                        if (!mbp.MOS1phi.Given)
                        {
                            mbp.MOS1phi.Value = 2 * vtnom * Math.Log(mbp.MOS1substrateDoping * 1e6 / 1.45e16);
                            mbp.MOS1phi.Value = Math.Max(.1, mbp.MOS1phi);
                        }
                        fermis = mbp.MOS1type * .5 * mbp.MOS1phi;
                        wkfng = 3.2;
                        if (!mbp.MOS1gateType.Given)
                            mbp.MOS1gateType.Value = 1;
                        if (mbp.MOS1gateType != 0)
                        {
                            fermig = mbp.MOS1type * mbp.MOS1gateType * .5 * egfet1;
                            wkfng = 3.25 + .5 * egfet1 - fermig;
                        }
                        wkfngs = wkfng - (3.25 + .5 * egfet1 + fermis);
                        if (!mbp.MOS1gamma.Given)
                        {
                            mbp.MOS1gamma.Value = Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Circuit.Charge * mbp.MOS1substrateDoping * 1e6) / MOS1oxideCapFactor;
                        }
                        if (!mbp.MOS1vt0.Given)
                        {
                            if (!mbp.MOS1surfaceStateDensity.Given)
                                mbp.MOS1surfaceStateDensity.Value = 0;
                            vfb = wkfngs - mbp.MOS1surfaceStateDensity * 1e4 * Circuit.Charge / MOS1oxideCapFactor;
                            mbp.MOS1vt0.Value = vfb + mbp.MOS1type * (mbp.MOS1gamma * Math.Sqrt(mbp.MOS1phi) + mbp.MOS1phi);
                        }
                    }
                    else
                    {
                        mbp.MOS1substrateDoping.Value = 0;
                        throw new CircuitException($"{Name}: Nsub < Ni");
                    }
                }
            }
        }
    }
}
