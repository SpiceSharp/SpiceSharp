using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Bipolars
{
    /// <summary>
    /// Temperature behavior for a <see cref="BipolarJunctionTransistorModel"/>.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITemperatureBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="ModelParameters"/>
    [BehaviorFor(typeof(BipolarJunctionTransistorModel)), AddBehaviorIfNo(typeof(ITemperatureBehavior))]
    public class ModelTemperature : Behavior,
        ITemperatureBehavior,
        IParameterized<ModelParameters>
    {
        private readonly ITemperatureSimulationState _temperature;

        /// <inheritdoc/>
        public ModelParameters Parameters { get; }

        /// <summary>
        /// Gets the inverse forward Early voltage.
        /// </summary>
        /// <value>
        /// The inverse forward Early voltage.
        /// </value>
        public double InverseEarlyVoltForward { get; protected set; }

        /// <summary>
        /// Gets the inverse reverse Early voltage.
        /// </summary>
        /// <value>
        /// The inverse reverse Early voltage.
        /// </value>
        public double InverseEarlyVoltReverse { get; protected set; }

        /// <summary>
        /// Gets the inverse forward roll-off current.
        /// </summary>
        /// <value>
        /// The inverse forward roll-off current.
        /// </value>
        public double InverseRollOffForward { get; protected set; }

        /// <summary>
        /// Gets the inverse reverse roll-off current.
        /// </summary>
        /// <value>
        /// The inverse reverse roll-off current.
        /// </value>
        public double InverseRollOffReverse { get; protected set; }

        /// <summary>
        /// Gets the collector conductance.
        /// </summary>
        /// <value>
        /// The collector conductance.
        /// </value>
        [ParameterName("collectorconduct"), ParameterInfo("Collector conductance")]
        public double CollectorConduct { get; protected set; }

        /// <summary>
        /// Gets the emitter conductance.
        /// </summary>
        /// <value>
        /// The emitter conductance.
        /// </value>
        [ParameterName("emitterconduct"), ParameterInfo("Emitter conductance")]
        public double EmitterConduct { get; protected set; }

        /// <summary>
        /// Gets the transit time base-collector voltage factor.
        /// </summary>
        /// <value>
        /// The transit time base-collector factor.
        /// </value>
        [ParameterName("transtimevbcfact"), ParameterInfo("Transit time VBC factor")]
        public double TransitTimeVoltageBcFactor { get; protected set; }

        /// <summary>
        /// Gets the excess phase factor.
        /// </summary>
        /// <value>
        /// The excess phase factor.
        /// </value>
        [ParameterName("excessphasefactor"), ParameterInfo("Excess phase factor")]
        public double ExcessPhaseFactor { get; protected set; }

        /// <summary>
        /// Gets generic factor 1.
        /// </summary>
        /// <value>
        /// The factor1.
        /// </value>
        public double Factor1 { get; protected set; }

        /// <summary>
        /// Gets ???.
        /// </summary>
        /// <value>
        /// The XFC.
        /// </value>
        public double Xfc { get; protected set; }

        /// <summary>
        /// Gets implementation-specific factor 2.
        /// </summary>
        /// <value>
        /// The f2.
        /// </value>
        public double F2 { get; protected set; }

        /// <summary>
        /// Gets implementation-specific factor 3.
        /// </summary>
        /// <value>
        /// The f3.
        /// </value>
        public double F3 { get; protected set; }

        /// <summary>
        /// Gets implementation-specific factor 6.
        /// </summary>
        /// <value>
        /// The f6.
        /// </value>
        public double F6 { get; protected set; }

        /// <summary>
        /// Gets implementation-specific 7.
        /// </summary>
        /// <value>
        /// The f7.
        /// </value>
        public double F7 { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelTemperature"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public ModelTemperature(IBindingContext context) : base(context)
        {
            context.ThrowIfNull(nameof(context));
            _temperature = context.GetState<ITemperatureSimulationState>();
            Parameters = context.GetParameterSet<ModelParameters>();
        }

        /// <inheritdoc/>
        void ITemperatureBehavior.Temperature()
        {
            if (!Parameters.NominalTemperature.Given)
                Parameters.NominalTemperature = new GivenParameter<double>(_temperature.NominalTemperature, false);
            Factor1 = Parameters.NominalTemperature / Constants.ReferenceTemperature;

            if (!Parameters.LeakBeCurrent.Given)
                Parameters.LeakBeCurrent = new GivenParameter<double>(Parameters.C2 * Parameters.SatCur, false);
            if (!Parameters.LeakBcCurrent.Given)
                Parameters.LeakBcCurrent = new GivenParameter<double>(Parameters.C4 * Parameters.SatCur, false);
            if (!Parameters.MinimumBaseResistance.Given)
                Parameters.MinimumBaseResistance = new GivenParameter<double>(Parameters.BaseResist, false);

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

            if (!Parameters.EarlyVoltageForward.Equals(0.0))
                InverseEarlyVoltForward = 1 / Parameters.EarlyVoltageForward;
            else
                InverseEarlyVoltForward = 0;
            if (!Parameters.RollOffForward.Equals(0.0))
                InverseRollOffForward = 1 / Parameters.RollOffForward;
            else
                InverseRollOffForward = 0;
            if (!Parameters.EarlyVoltageReverse.Equals(0.0))
                InverseEarlyVoltReverse = 1 / Parameters.EarlyVoltageReverse;
            else
                InverseEarlyVoltReverse = 0;
            if (!Parameters.RollOffReverse.Equals(0.0))
                InverseRollOffReverse = 1 / Parameters.RollOffReverse;
            else
                InverseRollOffReverse = 0;
            if (!Parameters.CollectorResistance.Equals(0.0))
                CollectorConduct = 1 / Parameters.CollectorResistance;
            else
                CollectorConduct = 0;
            if (!Parameters.EmitterResistance.Equals(0.0))
                EmitterConduct = 1 / Parameters.EmitterResistance;
            else
                EmitterConduct = 0;
            if (!Parameters.TransitTimeForwardVoltageBc.Equals(0.0))
                TransitTimeVoltageBcFactor = 1 / (Parameters.TransitTimeForwardVoltageBc * 1.44);
            else
                TransitTimeVoltageBcFactor = 0;
            ExcessPhaseFactor = Parameters.ExcessPhase / (180.0 / Math.PI) * Parameters.TransitTimeForward;
            Xfc = Math.Log(1 - Parameters.DepletionCapCoefficient);
            F2 = Math.Exp((1 + Parameters.JunctionExpBe) * Xfc);
            F3 = 1 - Parameters.DepletionCapCoefficient * (1 + Parameters.JunctionExpBe);
            F6 = Math.Exp((1 + Parameters.JunctionExpBc) * Xfc);
            F7 = 1 - Parameters.DepletionCapCoefficient * (1 + Parameters.JunctionExpBc);
        }
    }
}
