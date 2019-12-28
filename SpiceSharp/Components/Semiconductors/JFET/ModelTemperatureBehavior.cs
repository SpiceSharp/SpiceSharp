using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="JFETModel" />.
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
        /// Gets the implementation-specific factor 2.
        /// </summary>
        public double F2 { get; private set; }

        /// <summary>
        /// Gets the implementation-specific factor 3.
        /// </summary>
        public double F3 { get; private set; }

        /// <summary>
        /// Gets the bulk factor.
        /// </summary>
        public double BFactor { get; private set; }

        /// <summary>
        /// Gets the implementation-specific factor Pbo.
        /// </summary>
        public double Pbo { get; private set; }

        /// <summary>
        /// Gets ???
        /// </summary>
        public double Xfc { get; private set; }

        /// <summary>
        /// Gets the junction capacitance factor.
        /// </summary>
        public double Cjfact { get; private set; }

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
            BiasingState = context.GetState<IBiasingSimulationState>();
        }
        
        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            if (Parameters.NominalTemperature.Given)
                Parameters.NominalTemperature.RawValue = _temperature.NominalTemperature;

            var vtnom = Constants.KOverQ * Parameters.NominalTemperature;
            var fact1 = Parameters.NominalTemperature / Constants.ReferenceTemperature;
            var kt1 = Constants.Boltzmann * Parameters.NominalTemperature;
            var egfet1 = 1.16 - (7.02e-4 * Parameters.NominalTemperature * Parameters.NominalTemperature) /
                         (Parameters.NominalTemperature + 1108);
            var arg1 = -egfet1 / (kt1 + kt1) + 1.1150877 / (Constants.Boltzmann * 2 * Constants.ReferenceTemperature);
            var pbfact1 = -2 * vtnom * (1.5 * Math.Log(fact1) + Constants.Charge * arg1);
            Pbo = (Parameters.GatePotential - pbfact1) / fact1;
            var gmaold = (Parameters.GatePotential - Pbo) / Pbo;
            Cjfact = 1 / (1 + .5 * (4e-4 * (Parameters.NominalTemperature - Constants.ReferenceTemperature) - gmaold));

            if (Parameters.DepletionCapCoefficient > 0.95)
            {
                SpiceSharpWarning.Warning(this,
                    Properties.Resources.JFETs_DepletionCapCoefficientTooLarge.FormatString(Name, Parameters.DepletionCapCoefficient.Value));
                Parameters.DepletionCapCoefficient.Value = .95;
            }

            Xfc = Math.Log(1 - Parameters.DepletionCapCoefficient);
            F2 = Math.Exp((1 + 0.5) * Xfc);
            F3 = 1 - Parameters.DepletionCapCoefficient * (1 + 0.5);
            /* Modification for Sydney University JFET model */
            BFactor = (1 - Parameters.B) / (Parameters.GatePotential - Parameters.Threshold);
        }
    }
}
