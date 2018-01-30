using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="BipolarJunctionTransistorModel"/>
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
        [PropertyName("invearlyvoltf"), PropertyInfo("Inverse early voltage:forward")]
        public double InvEarlyVoltForward { get; internal set; }
        [PropertyName("invearlyvoltr"), PropertyInfo("Inverse early voltage:reverse")]
        public double InvEarlyVoltReverse { get; internal set; }
        [PropertyName("invrollofff"), PropertyInfo("Inverse roll off - forward")]
        public double InverseRollOffForward { get; internal set; }
        [PropertyName("invrolloffr"), PropertyInfo("Inverse roll off - reverse")]
        public double InverseRollOffReverse { get; internal set; }
        [PropertyName("collectorconduct"), PropertyInfo("Collector conductance")]
        public double CollectorConduct { get; internal set; }
        [PropertyName("emitterconduct"), PropertyInfo("Emitter conductance")]
        public double EmitterConduct { get; internal set; }
        [PropertyName("transtimevbcfact"), PropertyInfo("Transit time VBC factor")]
        public double TransitTimeVbcFactor { get; internal set; }
        [PropertyName("excessphasefactor"), PropertyInfo("Excess phase fact.")]
        public double ExcessPhaseFactor { get; internal set; }
        
        public double Fact1 { get; protected set; }
        public double Xfc { get; protected set; }

        public double F2 { get; protected set; }
        public double F3 { get; protected set; }
        public double F6 { get; protected set; }
        public double F7 { get; protected set; }

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

            if (!mbp.NominalTemperature.Given)
                mbp.NominalTemperature.Value = simulation.State.NominalTemperature;
            Fact1 = mbp.NominalTemperature / Circuit.ReferenceTemperature;

            if (!mbp.LeakBEcurrent.Given)
            {
                if (mbp.C2.Given)
                    mbp.LeakBEcurrent.Value = mbp.C2 * mbp.SatCur;
                else
                    mbp.LeakBEcurrent.Value = 0;
            }
            if (!mbp.LeakBCCurrent.Given)
            {
                if (mbp.C4.Given)
                    mbp.LeakBCCurrent.Value = mbp.C4 * mbp.SatCur;
                else
                    mbp.LeakBCCurrent.Value = 0;
            }
            if (!mbp.MinimumBaseResistance.Given)
                mbp.MinimumBaseResistance.Value = mbp.BaseResist;

            /* 
			 * COMPATABILITY WARNING!
			 * special note:  for backward compatability to much older models, spice 2G
			 * implemented a special case which checked if B - E leakage saturation
			 * current was >1, then it was instead a the B - E leakage saturation current
			 * divided by IS, and multiplied it by IS at this point.  This was not
			 * handled correctly in the 2G code, and there is some question on its 
			 * reasonability, since it is also undocumented, so it has been left out
			 * here.  It could easily be added with 1 line.  (The same applies to the B - C
			 * leakage saturation current).   TQ  6 / 29 / 84
			 */

            if (mbp.EarlyVoltagForward.Given && mbp.EarlyVoltagForward != 0)
                InvEarlyVoltForward = 1 / mbp.EarlyVoltagForward;
            else
                InvEarlyVoltForward = 0;
            if (mbp.RollOffForward.Given && mbp.RollOffForward != 0)
                InverseRollOffForward = 1 / mbp.RollOffForward;
            else
                InverseRollOffForward = 0;
            if (mbp.EarlyVoltageReverse.Given && mbp.EarlyVoltageReverse != 0)
                InvEarlyVoltReverse = 1 / mbp.EarlyVoltageReverse;
            else
                InvEarlyVoltReverse = 0;
            if (mbp.RollOffReverse.Given && mbp.RollOffReverse != 0)
                InverseRollOffReverse = 1 / mbp.RollOffReverse;
            else
                InverseRollOffReverse = 0;
            if (mbp.CollectorResistance.Given && mbp.CollectorResistance != 0)
                CollectorConduct = 1 / mbp.CollectorResistance;
            else
                CollectorConduct = 0;
            if (mbp.EmitterResistance.Given && mbp.EmitterResistance != 0)
                EmitterConduct = 1 / mbp.EmitterResistance;
            else
                EmitterConduct = 0;
            if (mbp.TransitTimeFVBC.Given && mbp.TransitTimeFVBC != 0)
                TransitTimeVbcFactor = 1 / (mbp.TransitTimeFVBC * 1.44);
            else
                TransitTimeVbcFactor = 0;
            ExcessPhaseFactor = (mbp.ExcessPhase / (180.0 / Math.PI)) * mbp.TransitTimeForward;
            if (mbp.DepletionCapCoefficient.Given)
            {
                if (mbp.DepletionCapCoefficient > 0.9999)
                {
                    mbp.DepletionCapCoefficient.Value = 0.9999;
                    throw new CircuitException("BJT model {0}, parameter fc limited to 0.9999".FormatString(Name));
                }
            }
            else
            {
                mbp.DepletionCapCoefficient.Value = .5;
            }
            Xfc = Math.Log(1 - mbp.DepletionCapCoefficient);
            F2 = Math.Exp((1 + mbp.JunctionExpBE) * Xfc);
            F3 = 1 - mbp.DepletionCapCoefficient * (1 + mbp.JunctionExpBE);
            F6 = Math.Exp((1 + mbp.JunctionExpBC) * Xfc);
            F7 = 1 - mbp.DepletionCapCoefficient * (1 + mbp.JunctionExpBC);
        }
    }
}
