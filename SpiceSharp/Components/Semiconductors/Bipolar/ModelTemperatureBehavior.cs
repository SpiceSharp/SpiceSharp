using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="BipolarJunctionTransistorModel"/>
    /// </summary>
    public class ModelTemperatureBehavior : Behavior, ITemperatureBehavior,
        IParameterized<ModelBaseParameters>,
        IParameterized<ModelNoiseParameters>
    {
        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public ModelBaseParameters Parameters { get; }

        /// <summary>
        /// Gets the noise parameters.
        /// </summary>
        /// <value>
        /// The noise parameters.
        /// </value>
        public ModelNoiseParameters NoiseParameters { get; }
        ModelNoiseParameters IParameterized<ModelNoiseParameters>.Parameters => NoiseParameters;

        /// <summary>
        /// Gets the inverse Early voltage (forward).
        /// </summary>
        [ParameterName("invearlyvoltf"), ParameterInfo("Inverse Early voltage (forward)")]
        public double InverseEarlyVoltForward { get; protected set; }

        /// <summary>
        /// Gets the inverse Early voltage (reverse).
        /// </summary>
        [ParameterName("invearlyvoltr"), ParameterInfo("Inverse Early voltage (reverse)")]
        public double InverseEarlyVoltReverse { get; protected set; }

        /// <summary>
        /// Gets the inverse roll-off (forward).
        /// </summary>
        [ParameterName("invrollofff"), ParameterInfo("Inverse roll off (forward)")]
        public double InverseRollOffForward { get; protected set; }

        /// <summary>
        /// Gets the inverse roll-off (reverse).
        /// </summary>
        [ParameterName("invrolloffr"), ParameterInfo("Inverse roll off (reverse)")]
        public double InverseRollOffReverse { get; protected set; }

        /// <summary>
        /// Gets the collector conductance.
        /// </summary>
        [ParameterName("collectorconduct"), ParameterInfo("Collector conductance")]
        public double CollectorConduct { get; protected set; }

        /// <summary>
        /// Gets the emitter conductance.
        /// </summary>
        [ParameterName("emitterconduct"), ParameterInfo("Emitter conductance")]
        public double EmitterConduct { get; protected set; }

        /// <summary>
        /// Gets the transit time base-collector voltage factor.
        /// </summary>
        [ParameterName("transtimevbcfact"), ParameterInfo("Transit time VBC factor")]
        public double TransitTimeVoltageBcFactor { get; protected set; }

        /// <summary>
        /// Gets the excess phase factor.
        /// </summary>
        [ParameterName("excessphasefactor"), ParameterInfo("Excess phase factor")]
        public double ExcessPhaseFactor { get; protected set; }
        
        /// <summary>
        /// Gets generic factor 1.
        /// </summary>
        public double Factor1 { get; protected set; }

        /// <summary>
        /// Gets ???.
        /// </summary>
        public double Xfc { get; protected set; }

        /// <summary>
        /// Gets implementation-specific factor 2.
        /// </summary>
        public double F2 { get; protected set; }

        /// <summary>
        /// Gets implementation-specific factor 3.
        /// </summary>
        public double F3 { get; protected set; }

        /// <summary>
        /// Gets implementation-specific factor 6.
        /// </summary>
        public double F6 { get; protected set; }

        /// <summary>
        /// Gets implementation-specific 7.
        /// </summary>
        public double F7 { get; protected set; }

        /// <summary>
        /// Gets the biasing simulation state.
        /// </summary>
        /// <value>
        /// The biasing simulation state.
        /// </value>
        protected IBiasingSimulationState BiasingState { get; private set; }
        private readonly ITemperatureSimulationState _temperature;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelTemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public ModelTemperatureBehavior(string name, ModelBindingContext context) : base(name) 
        {
            context.ThrowIfNull(nameof(context));
            _temperature = context.GetState<ITemperatureSimulationState>();
            Parameters = context.GetParameterSet<ModelBaseParameters>();
            NoiseParameters = context.GetParameterSet<ModelNoiseParameters>();
            BiasingState = context.GetState<IBiasingSimulationState>();
        }

        /// <summary>
        /// Do temperature-dependent calculations.
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            if (!Parameters.NominalTemperature.Given)
                Parameters.NominalTemperature.RawValue = _temperature.NominalTemperature;
            Factor1 = Parameters.NominalTemperature / Constants.ReferenceTemperature;

            if (!Parameters.LeakBeCurrent.Given)
            {
                if (Parameters.C2.Given)
                    Parameters.LeakBeCurrent.RawValue = Parameters.C2 * Parameters.SatCur;
                else
                    Parameters.LeakBeCurrent.RawValue = 0;
            }
            if (!Parameters.LeakBcCurrent.Given)
            {
                if (Parameters.C4.Given)
                    Parameters.LeakBcCurrent.RawValue = Parameters.C4 * Parameters.SatCur;
                else
                    Parameters.LeakBcCurrent.RawValue = 0;
            }
            if (!Parameters.MinimumBaseResistance.Given)
                Parameters.MinimumBaseResistance.RawValue = Parameters.BaseResist;

            /* 
			 * COMPATABILITY WARNING!
			 * special note: for backward compatability to much older models, spice 2G
			 * implemented a special case which checked if B-E leakage saturation
			 * current was >1, then it was instead a the B-E leakage saturation current
			 * divided by IS, and multiplied it by IS at this point. This was not
			 * handled correctly in the 2G code, and there is some question on its 
			 * reasonability, since it is also undocumented, so it has been left out
			 * here. It could easily be added with 1 line. (The same applies to the B-C
			 * leakage saturation current).   TQ  6/29/84
			 */

            if (Parameters.EarlyVoltageForward.Given && !Parameters.EarlyVoltageForward.Value.Equals(0.0))
                InverseEarlyVoltForward = 1 / Parameters.EarlyVoltageForward;
            else
                InverseEarlyVoltForward = 0;
            if (Parameters.RollOffForward.Given && !Parameters.RollOffForward.Value.Equals(0.0))
                InverseRollOffForward = 1 / Parameters.RollOffForward;
            else
                InverseRollOffForward = 0;
            if (Parameters.EarlyVoltageReverse.Given && !Parameters.EarlyVoltageReverse.Value.Equals(0.0))
                InverseEarlyVoltReverse = 1 / Parameters.EarlyVoltageReverse;
            else
                InverseEarlyVoltReverse = 0;
            if (Parameters.RollOffReverse.Given && !Parameters.RollOffReverse.Value.Equals(0.0))
                InverseRollOffReverse = 1 / Parameters.RollOffReverse;
            else
                InverseRollOffReverse = 0;
            if (Parameters.CollectorResistance > 0)
                CollectorConduct = 1 / Parameters.CollectorResistance;
            else
                CollectorConduct = 0;
            if (Parameters.EmitterResistance > 0)
                EmitterConduct = 1 / Parameters.EmitterResistance;
            else
                EmitterConduct = 0;
            if (Parameters.TransitTimeForwardVoltageBc > 0)
                TransitTimeVoltageBcFactor = 1 / (Parameters.TransitTimeForwardVoltageBc * 1.44);
            else
                TransitTimeVoltageBcFactor = 0;
            ExcessPhaseFactor = Parameters.ExcessPhase / (180.0 / Math.PI) * Parameters.TransitTimeForward;
            if (Parameters.DepletionCapCoefficient.Given)
            {
                if (Parameters.DepletionCapCoefficient > 0.9999)
                {
                    Parameters.DepletionCapCoefficient.RawValue = 0.9999;
                    SpiceSharpWarning.Warning(this,
                        Properties.Resources.BJTs_DepletionCapCoefficientTooLarge.FormatString(Name, Parameters.DepletionCapCoefficient.Value));
                }
            }
            else
            {
                Parameters.DepletionCapCoefficient.RawValue = 0.5;
            }
            Xfc = Math.Log(1 - Parameters.DepletionCapCoefficient);
            F2 = Math.Exp((1 + Parameters.JunctionExpBe) * Xfc);
            F3 = 1 - Parameters.DepletionCapCoefficient * (1 + Parameters.JunctionExpBe);
            F6 = Math.Exp((1 + Parameters.JunctionExpBc) * Xfc);
            F7 = 1 - Parameters.DepletionCapCoefficient * (1 + Parameters.JunctionExpBc);
        }
    }
}
