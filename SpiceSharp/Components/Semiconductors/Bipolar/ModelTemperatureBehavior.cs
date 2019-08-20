using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="BipolarJunctionTransistorModel"/>
    /// </summary>
    public class ModelTemperatureBehavior : Behavior, ITemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private ModelBaseParameters _mbp;

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
        /// Creates a new instance of the <see cref="ModelTemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public ModelTemperatureBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">Data provider</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Get parameters
            _mbp = context.GetParameterSet<ModelBaseParameters>();
        }

        /// <summary>
        /// Do temperature-dependent calculations.
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            if (!_mbp.NominalTemperature.Given)
                _mbp.NominalTemperature.RawValue = ((BaseSimulation)Simulation).RealState.NominalTemperature;
            Factor1 = _mbp.NominalTemperature / Constants.ReferenceTemperature;

            if (!_mbp.LeakBeCurrent.Given)
            {
                if (_mbp.C2.Given)
                    _mbp.LeakBeCurrent.RawValue = _mbp.C2 * _mbp.SatCur;
                else
                    _mbp.LeakBeCurrent.RawValue = 0;
            }
            if (!_mbp.LeakBcCurrent.Given)
            {
                if (_mbp.C4.Given)
                    _mbp.LeakBcCurrent.RawValue = _mbp.C4 * _mbp.SatCur;
                else
                    _mbp.LeakBcCurrent.RawValue = 0;
            }
            if (!_mbp.MinimumBaseResistance.Given)
                _mbp.MinimumBaseResistance.RawValue = _mbp.BaseResist;

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

            if (_mbp.EarlyVoltageForward.Given && !_mbp.EarlyVoltageForward.Value.Equals(0.0))
                InverseEarlyVoltForward = 1 / _mbp.EarlyVoltageForward;
            else
                InverseEarlyVoltForward = 0;
            if (_mbp.RollOffForward.Given && !_mbp.RollOffForward.Value.Equals(0.0))
                InverseRollOffForward = 1 / _mbp.RollOffForward;
            else
                InverseRollOffForward = 0;
            if (_mbp.EarlyVoltageReverse.Given && !_mbp.EarlyVoltageReverse.Value.Equals(0.0))
                InverseEarlyVoltReverse = 1 / _mbp.EarlyVoltageReverse;
            else
                InverseEarlyVoltReverse = 0;
            if (_mbp.RollOffReverse.Given && !_mbp.RollOffReverse.Value.Equals(0.0))
                InverseRollOffReverse = 1 / _mbp.RollOffReverse;
            else
                InverseRollOffReverse = 0;
            if (_mbp.CollectorResistance.Given && !_mbp.CollectorResistance.Value.Equals(0.0))
                CollectorConduct = 1 / _mbp.CollectorResistance;
            else
                CollectorConduct = 0;
            if (_mbp.EmitterResistance.Given && !_mbp.EmitterResistance.Value.Equals(0.0))
                EmitterConduct = 1 / _mbp.EmitterResistance;
            else
                EmitterConduct = 0;
            if (_mbp.TransitTimeForwardVoltageBc.Given && !_mbp.TransitTimeForwardVoltageBc.Value.Equals(0.0))
                TransitTimeVoltageBcFactor = 1 / (_mbp.TransitTimeForwardVoltageBc * 1.44);
            else
                TransitTimeVoltageBcFactor = 0;
            ExcessPhaseFactor = _mbp.ExcessPhase / (180.0 / Math.PI) * _mbp.TransitTimeForward;
            if (_mbp.DepletionCapCoefficient.Given)
            {
                if (_mbp.DepletionCapCoefficient > 0.9999)
                {
                    _mbp.DepletionCapCoefficient.RawValue = 0.9999;
                    throw new CircuitException("BJT model {0}, parameter fc limited to 0.9999".FormatString(Name));
                }
            }
            else
            {
                _mbp.DepletionCapCoefficient.RawValue = 0.5;
            }
            Xfc = Math.Log(1 - _mbp.DepletionCapCoefficient);
            F2 = Math.Exp((1 + _mbp.JunctionExpBe) * Xfc);
            F3 = 1 - _mbp.DepletionCapCoefficient * (1 + _mbp.JunctionExpBe);
            F6 = Math.Exp((1 + _mbp.JunctionExpBc) * Xfc);
            F7 = 1 - _mbp.DepletionCapCoefficient * (1 + _mbp.JunctionExpBc);
        }
    }
}
