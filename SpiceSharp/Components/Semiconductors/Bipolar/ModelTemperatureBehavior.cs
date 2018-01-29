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
        public double InvEarlyVoltF { get; internal set; }
        [PropertyName("invearlyvoltr"), PropertyInfo("Inverse early voltage:reverse")]
        public double InvEarlyVoltR { get; internal set; }
        [PropertyName("invrollofff"), PropertyInfo("Inverse roll off - forward")]
        public double InvRollOffF { get; internal set; }
        [PropertyName("invrolloffr"), PropertyInfo("Inverse roll off - reverse")]
        public double InvRollOffR { get; internal set; }
        [PropertyName("collectorconduct"), PropertyInfo("Collector conductance")]
        public double CollectorConduct { get; internal set; }
        [PropertyName("emitterconduct"), PropertyInfo("Emitter conductance")]
        public double EmitterConduct { get; internal set; }
        [PropertyName("transtimevbcfact"), PropertyInfo("Transit time VBC factor")]
        public double TransitTimeVBCFactor { get; internal set; }
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
            if (!mbp.LeakBCcurrent.Given)
            {
                if (mbp.C4.Given)
                    mbp.LeakBCcurrent.Value = mbp.C4 * mbp.SatCur;
                else
                    mbp.LeakBCcurrent.Value = 0;
            }
            if (!mbp.MinBaseResist.Given)
                mbp.MinBaseResist.Value = mbp.BaseResist;

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

            if (mbp.EarlyVoltF.Given && mbp.EarlyVoltF != 0)
                InvEarlyVoltF = 1 / mbp.EarlyVoltF;
            else
                InvEarlyVoltF = 0;
            if (mbp.RollOffF.Given && mbp.RollOffF != 0)
                InvRollOffF = 1 / mbp.RollOffF;
            else
                InvRollOffF = 0;
            if (mbp.EarlyVoltR.Given && mbp.EarlyVoltR != 0)
                InvEarlyVoltR = 1 / mbp.EarlyVoltR;
            else
                InvEarlyVoltR = 0;
            if (mbp.RollOffR.Given && mbp.RollOffR != 0)
                InvRollOffR = 1 / mbp.RollOffR;
            else
                InvRollOffR = 0;
            if (mbp.CollectorResist.Given && mbp.CollectorResist != 0)
                CollectorConduct = 1 / mbp.CollectorResist;
            else
                CollectorConduct = 0;
            if (mbp.EmitterResist.Given && mbp.EmitterResist != 0)
                EmitterConduct = 1 / mbp.EmitterResist;
            else
                EmitterConduct = 0;
            if (mbp.TransitTimeFVBC.Given && mbp.TransitTimeFVBC != 0)
                TransitTimeVBCFactor = 1 / (mbp.TransitTimeFVBC * 1.44);
            else
                TransitTimeVBCFactor = 0;
            ExcessPhaseFactor = (mbp.ExcessPhase / (180.0 / Math.PI)) * mbp.TransitTimeF;
            if (mbp.DepletionCapCoeff.Given)
            {
                if (mbp.DepletionCapCoeff > 0.9999)
                {
                    mbp.DepletionCapCoeff.Value = 0.9999;
                    throw new CircuitException("BJT model {0}, parameter fc limited to 0.9999".FormatString(Name));
                }
            }
            else
            {
                mbp.DepletionCapCoeff.Value = .5;
            }
            Xfc = Math.Log(1 - mbp.DepletionCapCoeff);
            F2 = Math.Exp((1 + mbp.JunctionExpBE) * Xfc);
            F3 = 1 - mbp.DepletionCapCoeff * (1 + mbp.JunctionExpBE);
            F6 = Math.Exp((1 + mbp.JunctionExpBC) * Xfc);
            F7 = 1 - mbp.DepletionCapCoeff * (1 + mbp.JunctionExpBC);
        }
    }
}
