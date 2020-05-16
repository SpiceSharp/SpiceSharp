﻿using System;
using SpiceSharp.ParameterSets;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Resistors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Resistor"/>.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITemperatureBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="Parameters"/>
    public class Temperature : Behavior,
        ITemperatureBehavior,
        IParameterized<Parameters>
    {
        private readonly ITemperatureSimulationState _temperature;
        private readonly ModelParameters _mbp = null;

        /// <inheritdoc/>
        public Parameters Parameters { get; }

        /// <summary>
        /// Gets the conductance for this resistor.
        /// </summary>
        /// <value>
        /// The conductance.
        /// </value>
        [ParameterName("g"), ParameterInfo("The conductance of the resistor.")]
        public double Conductance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Temperature"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public Temperature(string name, ComponentBindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));
            _temperature = context.GetState<ITemperatureSimulationState>();
            Parameters = context.GetParameterSet<Parameters>();
            if (context.ModelBehaviors != null)
                _mbp = context.ModelBehaviors.GetParameterSet<ModelParameters>();
        }

        void ITemperatureBehavior.Temperature()
        {
            double factor;
            double resistance = Parameters.Resistance;

            // Default Value Processing for Resistor Instance
            if (!Parameters.Temperature.Given)
                Parameters.Temperature = new GivenParameter<double>(_temperature.Temperature, false);

            if (_mbp != null)
            {
                if (!Parameters.Resistance.Given)
                {
                    if (!Parameters.Width.Given)
                        Parameters.Width = new GivenParameter<double>(_mbp.DefaultWidth, false);

                    if (!_mbp.SheetResistance.Equals(0.0) && Parameters.Length > 0)
                        resistance = _mbp.SheetResistance * (Parameters.Length - _mbp.Narrow) / (Parameters.Width - _mbp.Narrow);
                    else
                    {
                        SpiceSharpWarning.Warning(this, Properties.Resources.Resistors_ZeroResistance.FormatString(Name));
                        resistance = 1000;
                    }
                }

                var difference = Parameters.Temperature - _mbp.NominalTemperature;

                if (_mbp.ExponentialCoefficient.Given)
                    factor = Math.Pow(1.01, _mbp.ExponentialCoefficient * difference);
                else
                    factor = 1.0 + _mbp.TemperatureCoefficient1 * difference + _mbp.TemperatureCoefficient2 * difference * difference;
            }
            else
            {
                factor = 1.0;
            }

            if (resistance < Parameters.MinimumResistance)
            {
                SpiceSharpWarning.Warning(this, Properties.Resources.Parameters_NotGreaterOrEqual.FormatString("resistance", resistance, Parameters.MinimumResistance));
                resistance = Parameters.MinimumResistance;
            }

            // Calculate the final conductance
            Conductance = Parameters.ParallelMultiplier / Parameters.SeriesMultiplier / (resistance * factor);
        }
    }
}