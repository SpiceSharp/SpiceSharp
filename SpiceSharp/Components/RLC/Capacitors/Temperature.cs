﻿using SpiceSharp.ParameterSets;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Capacitors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Capacitor" />.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITemperatureBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="Capacitors.Parameters"/>
    public class Temperature : Behavior,
        ITemperatureBehavior,
        IParameterized<Parameters>
    {
        private readonly ModelParameters _mbp = null;
        private readonly ITemperatureSimulationState _temperature;

        /// <inheritdoc/>
        public Parameters Parameters { get; }

        /// <summary>
        /// Gets the capacitance.
        /// </summary>
        /// <value>
        /// The capacitance.
        /// </value>
        [ParameterName("capacitance"), ParameterInfo("The capacitance of the capacitor.")]
        public double Capacitance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Temperature"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public Temperature(string name, IComponentBindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));

            // Get parameters
            Parameters = context.GetParameterSet<Parameters>();
            if (context.ModelBehaviors != null)
                _mbp = context.ModelBehaviors.GetParameterSet<ModelParameters>();

            // Connections
            _temperature = context.GetState<ITemperatureSimulationState>();
        }

        void ITemperatureBehavior.Temperature()
        {
            if (!Parameters.Temperature.Given)
                Parameters.Temperature = new GivenParameter<double>(_temperature.Temperature, false);

            double capacitance;
            if (!Parameters.Capacitance.Given)
            {
                // We need a model!
                if (_mbp == null)
                    throw new SpiceSharpException(Properties.Resources.Components_NoModel.FormatString(Name));

                var width = Parameters.Width.Given
                    ? Parameters.Width.Value
                    : _mbp.DefaultWidth;
                capacitance = _mbp.JunctionCap *
                              (width - _mbp.Narrow) *
                              (Parameters.Length - _mbp.Narrow) +
                              _mbp.JunctionCapSidewall * 2 * (
                                  Parameters.Length - _mbp.Narrow +
                                  (width - _mbp.Narrow));
            }
            else
                capacitance = Parameters.Capacitance;

            double factor = 1.0;

            if (_mbp != null)
            {
                double temperatureDiff = Parameters.Temperature - _mbp.NominalTemperature;
                factor = 1.0 + _mbp.TemperatureCoefficient1 * temperatureDiff + _mbp.TemperatureCoefficient2 * temperatureDiff * temperatureDiff;
            }

            Capacitance = factor * capacitance * Parameters.ParallelMultiplier;
        }
    }
}