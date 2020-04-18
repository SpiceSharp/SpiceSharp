using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    [GeneratedParameters]
    public class BaseParameters : ParameterSet
    {
        private double _area = 1;
        private GivenParameter<double> _temperature = new GivenParameter<double>(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Gets or sets the temperature in degrees Celsius.
        /// </summary>
        [ParameterName("temp"), ParameterInfo("Instance temperature", Units = "\u00b0C")]
        [DerivedProperty(), GreaterThan(Constants.CelsiusKelvin)]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the temperature parameter in degrees Kelvin.
        /// </summary>
        [GreaterThan(0)]
        public GivenParameter<double> Temperature
        {
            get => _temperature;
            set
            {
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(Temperature), value, 0));
                _temperature = value;
            }
        }

        /// <summary>
        /// Gets the area parameter.
        /// </summary>
        [ParameterName("area"), ParameterInfo("Area factor", Units = "m^2")]
        [GreaterThan(0)]
        public double Area
        {
            get => _area;
            set
            {
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(Area), value, 0));
                _area = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not the device is initially off (non-conducting).
        /// </summary>
        [ParameterName("off"), ParameterInfo("Device initially off")]
        public bool Off { get; set; }

        /// <summary>
        /// Gets the initial base-emitter voltage parameter.
        /// </summary>
        [ParameterName("icvbe"), ParameterInfo("Initial B-E voltage", Units = "V")]
        public double InitialVoltageBe { get; set; }

        /// <summary>
        /// Gets the initial collector-emitter voltage parameter.
        /// </summary>
        [ParameterName("icvce"), ParameterInfo("Initial C-E voltage", Units = "V")]
        public double InitialVoltageCe { get; set; }

        /// <summary>
        /// Set initial conditions of the device.
        /// </summary>
        /// <param name="value">The initial voltages (Vce, Vbe) or just (Vbe).</param>
        [ParameterName("ic"), ParameterInfo("Initial condition vector", Units = "V")]
        public void SetIc(double[] value)
        {
            value.ThrowIfNull(nameof(value));

            switch (value.Length)
            {
                case 2:
                    InitialVoltageCe = value[1];
                    goto case 1;
                case 1:
                    InitialVoltageBe = value[0];
                    break;
                default:
                    throw new BadParameterException(nameof(value));
            }
        }
    }
}
