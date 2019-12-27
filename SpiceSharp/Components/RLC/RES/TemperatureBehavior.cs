using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Resistor"/>
    /// </summary>
    public class TemperatureBehavior : Behavior, ITemperatureBehavior,
        IParameterized<BaseParameters>
    {
        private readonly ITemperatureSimulationState _temperature;
        private readonly ModelBaseParameters _mbp = null;

        /// <summary>
        /// The minimum resistance for any resistor.
        /// </summary>
        protected const double MinimumResistance = 1e-12;

        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        /// <value>
        /// The base parameters.
        /// </value>
        public BaseParameters BaseParameters { get; }

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        BaseParameters IParameterized<BaseParameters>.Parameters => BaseParameters;

        /// <summary>
        /// Gets the default conductance for this model
        /// </summary>
        [ParameterName("g"), ParameterInfo("The conductance of the resistor.")]
        public double Conductance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TemperatureBehavior(string name, ComponentBindingContext context) : base(name) 
        {
            context.ThrowIfNull(nameof(context));
            _temperature = context.GetState<ITemperatureSimulationState>();
            BaseParameters = context.GetParameterSet<BaseParameters>();
            if (context.ModelBehaviors != null)
                _mbp = context.ModelBehaviors.GetParameterSet<ModelBaseParameters>();
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            double factor;
            double resistance = BaseParameters.Resistance;

            // Default Value Processing for Resistor Instance
            if (!BaseParameters.Temperature.Given)
                BaseParameters.Temperature.RawValue = _temperature.Temperature;
            if (!BaseParameters.Width.Given)
                BaseParameters.Width.RawValue = _mbp?.DefaultWidth ?? 0.0;

            if (_mbp != null)
            {
                if (!BaseParameters.Resistance.Given)
                {
                    if (_mbp.SheetResistance.Given && _mbp.SheetResistance > 0 && BaseParameters.Length > 0)
                        resistance = _mbp.SheetResistance * (BaseParameters.Length - _mbp.Narrow) / (BaseParameters.Width - _mbp.Narrow);
                    else
                    {
                        SpiceSharpWarning.Warning(this, Properties.Resources.Resistors_ZeroResistance.FormatString(Name));
                        resistance = 1000;
                    }
                }

                var difference = BaseParameters.Temperature - _mbp.NominalTemperature;

                if (_mbp.ExponentialCoefficient.Given)
                    factor = Math.Pow(1.01, _mbp.ExponentialCoefficient * difference);
                else
                    factor = 1.0 + _mbp.TemperatureCoefficient1 * difference + _mbp.TemperatureCoefficient2 * difference * difference;
            }
            else
            {
                factor = 1.0;
            }

            if (resistance < MinimumResistance)
                resistance = MinimumResistance;

            // Calculate the final conductance
            Conductance = BaseParameters.ParallelMultiplier / BaseParameters.SeriesMultiplier / (resistance * factor);
        }
    }
}
