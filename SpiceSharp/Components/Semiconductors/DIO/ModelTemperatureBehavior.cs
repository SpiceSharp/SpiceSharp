using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="DiodeModel"/>
    /// </summary>
    public class ModelTemperatureBehavior : Behavior, ITemperatureBehavior,
        IParameterized<ModelBaseParameters>,
        IParameterized<ModelNoiseParameters>
    {
        /// <summary>
        /// Gets the noise parameters.
        /// </summary>
        /// <value>
        /// The noise parameters.
        /// </value>
        public ModelNoiseParameters NoiseParameters { get; }

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        ModelNoiseParameters IParameterized<ModelNoiseParameters>.Parameters => NoiseParameters;

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public ModelBaseParameters Parameters { get; }

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

        private readonly ITemperatureSimulationState _temperature;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelTemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public ModelTemperatureBehavior(string name, IBindingContext context) : base(name) 
        {
            context.ThrowIfNull(nameof(context));
            NoiseParameters = context.GetParameterSet<ModelNoiseParameters>();
            _temperature = context.GetState<ITemperatureSimulationState>();
            Parameters = context.GetParameterSet<ModelBaseParameters>();
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            if (!Parameters.NominalTemperature.Given)
                Parameters.NominalTemperature = new GivenParameter<double>(_temperature.NominalTemperature, false);
            VtNominal = Constants.KOverQ * Parameters.NominalTemperature;

            // limit grading coeff to max of 0.9
            if (Parameters.GradingCoefficient > 0.9)
            {
                Parameters.GradingCoefficient = 0.9;
                SpiceSharpWarning.Warning(this, 
                    Properties.Resources.Diodes_GradingCoefficientTooLarge.FormatString(Name, Parameters.GradingCoefficient));
            }

            // limit activation energy to min of 0.1
            if (Parameters.ActivationEnergy < 0.1)
            {
                Parameters.ActivationEnergy = 0.1;
                SpiceSharpWarning.Warning(this, 
                    Properties.Resources.Diodes_ActivationEnergyTooSmall.FormatString(Name, Parameters.ActivationEnergy));
            }

            // limit depletion cap coeff to max of .95
            if (Parameters.DepletionCapCoefficient > 0.95)
            {
                Parameters.DepletionCapCoefficient = 0.95;
                SpiceSharpWarning.Warning(this, 
                    Properties.Resources.Diodes_DepletionCapCoefficientTooLarge.FormatString(Name, Parameters.DepletionCapCoefficient));
            }

            if (!Parameters.Resistance.Equals(0.0))
                Conductance = 1 / Parameters.Resistance;
            else
                Conductance = 0;
            Xfc = Math.Log(1 - Parameters.DepletionCapCoefficient);

            F2 = Math.Exp((1 + Parameters.GradingCoefficient) * Xfc);
            F3 = 1 - Parameters.DepletionCapCoefficient * (1 + Parameters.GradingCoefficient);
        }
    }
}
