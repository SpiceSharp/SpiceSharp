﻿using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Parameters for the capacitor model
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the bottom junction capacitance parameter.
        /// </summary>
        [ParameterName("cj"), ParameterInfo("Bottom capacitance per area", Units = "F/m^2")]
        public double JunctionCap { get; set; }

        /// <summary>
        /// Gets the junction sidewall capacitance parameter.
        /// </summary>
        [ParameterName("cjsw"), ParameterInfo("Sidewall capacitance per meter", Units = "F/m")]
        public double JunctionCapSidewall { get; set; }

        /// <summary>
        /// Gets the default width parameter.
        /// </summary>
        [ParameterName("defw"), ParameterInfo("Default width", Units = "m")]
        public double DefaultWidth { get; set; } = 10e-6;

        /// <summary>
        /// Gets the width correction factor parameter.
        /// </summary>
        [ParameterName("narrow"), ParameterInfo("Width correction factor", Units = "m")]
        public double Narrow { get; set; }

        /// <summary>
        /// Gets the first-order temperature coefficient parameter.
        /// </summary>
        [ParameterName("tc1"), ParameterInfo("First order temperature coefficient", Units = "F/K")]
        public double TemperatureCoefficient1 { get; set; }

        /// <summary>
        /// Gets the second-order temperature coefficient parameter.
        /// </summary>
        [ParameterName("tc2"), ParameterInfo("Second order temperature coefficient", Units = "F/K^2")]
        public double TemperatureCoefficient2 { get; set; }

        /// <summary>
        /// Gets or sets the nominal temperature in degrees Celsius.
        /// </summary>
        [ParameterName("tnom"), DerivedProperty(), ParameterInfo("Parameter measurement temperature", Units = "\u00b0C", Interesting = false)]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the nominal temperature parameter in degrees Kelvin.
        /// </summary>
        public GivenParameter<double> NominalTemperature { get; set; } = new GivenParameter<double>(Constants.ReferenceTemperature, false);
    }
}
