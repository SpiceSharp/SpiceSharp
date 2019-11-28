using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="DiodeModel"/>
    /// </summary>
    public class ModelTemperatureBehavior : Behavior, ITemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private ModelBaseParameters _mbp;

        /// <summary>
        /// Conductance
        /// </summary>
        [ParameterName("cond"), ParameterInfo("Ohmic conductance")]
        public double Conductance { get; protected set; }

        /// <summary>
        /// Gets the nominal thermal voltage.
        /// </summary>
        public double VtNominal { get; protected set; }

        /// <summary>
        /// Gets ???
        /// </summary>
        public double Xfc { get; protected set; }

        /// <summary>
        /// Gets the implementation-specific factor 2.
        /// </summary>
        public double F2 { get; protected set; }

        /// <summary>
        /// Gets the implementation-specific factor 3.
        /// </summary>
        public double F3 { get; protected set; }

        /// <summary>
        /// Gets the biasing simulation state.
        /// </summary>
        /// <value>
        /// The biasing simulation state.
        /// </value>
        protected IBiasingSimulationState BiasingState { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelTemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public ModelTemperatureBehavior(string name, ModelBindingContext context) : base(name) 
        {
            context.ThrowIfNull(nameof(context));

            _mbp = context.Behaviors.Parameters.GetValue<ModelBaseParameters>();
            BiasingState = context.GetState<IBiasingSimulationState>();
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            if (!_mbp.NominalTemperature.Given)
                _mbp.NominalTemperature.RawValue = BiasingState.NominalTemperature;
            VtNominal = Constants.KOverQ * _mbp.NominalTemperature;

            // limit grading coeff to max of 0.9
            if (_mbp.GradingCoefficient > 0.9)
            {
                _mbp.GradingCoefficient.RawValue = 0.9;
                SpiceSharpWarning.Warning(this, 
                    Properties.Resources.Diodes_GradingCoefficientTooLarge.FormatString(Name, _mbp.GradingCoefficient.Value));
            }

            // limit activation energy to min of 0.1
            if (_mbp.ActivationEnergy < 0.1)
            {
                _mbp.ActivationEnergy.RawValue = 0.1;
                SpiceSharpWarning.Warning(this, 
                    Properties.Resources.Diodes_ActivationEnergyTooSmall.FormatString(Name, _mbp.ActivationEnergy.Value));
            }

            // limit depletion cap coeff to max of .95
            if (_mbp.DepletionCapCoefficient > 0.95)
            {
                _mbp.DepletionCapCoefficient.RawValue = 0.95;
                SpiceSharpWarning.Warning(this, 
                    Properties.Resources.Diodes_DepletionCapCoefficientTooLarge.FormatString(Name, _mbp.DepletionCapCoefficient.Value));
            }

            if (_mbp.Resistance > 0)
                Conductance = 1 / _mbp.Resistance;
            else
                Conductance = 0;
            Xfc = Math.Log(1 - _mbp.DepletionCapCoefficient);

            F2 = Math.Exp((1 + _mbp.GradingCoefficient) * Xfc);
            F3 = 1 - _mbp.DepletionCapCoefficient * (1 + _mbp.GradingCoefficient);
        }
    }
}
