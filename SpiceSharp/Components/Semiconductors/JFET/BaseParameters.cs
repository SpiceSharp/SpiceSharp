using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public class BaseParameters : ParameterSet
    {
        private double _area = 1;
        private GivenParameter<double> _temperature = new GivenParameter<double>(300.15, false);
        private double _temperatureCelsius;

        /// <summary>
        /// Gets or sets the temperature in degrees celsius.
        /// </summary>
        [ParameterName("temp"), ParameterInfo("Instance temperature", Units = "\u00b0C")]
        [GreaterThan(Constants.CelsiusKelvin)]
        public double TemperatureCelsius
        {
            get => _temperatureCelsius;
            set
            {
                Utility.GreaterThan(value, nameof(TemperatureCelsius), Constants.CelsiusKelvin);
                _temperatureCelsius = value;
            }
        }

        /// <summary>
        /// Gets the temperature in Kelvin.
        /// </summary>
        [GreaterThan(0)]
        public GivenParameter<double> Temperature
        {
            get => _temperature;
            set
            {
                Utility.GreaterThan(value, nameof(Temperature), 0);
                _temperature = value;
            }
        }

        /// <summary>
        /// Gets the area.
        /// </summary>
        [ParameterName("area"), ParameterInfo("Area factor", Units = "m^2")]
        [GreaterThanOrEquals(0)]
        public double Area
        {
            get => _area;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Area), 0);
                _area = value;
            }
        }

        /// <summary>
        /// Gets the initial D-S voltage.
        /// </summary>
        [ParameterName("ic-vds"), ParameterInfo("Initial D-S voltage", Units = "V")]
        public double InitialVds { get; set; }

        /// <summary>
        /// Gets the initial G-S voltage.
        /// </summary>
        [ParameterName("ic-vgs"), ParameterInfo("Initial G-S voltage", Units = "V")]
        public double InitialVgs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is off.
        /// </summary>
        [ParameterName("off"), ParameterInfo("Device initially off")]
        public bool Off { get; set; }

        /// <summary>
        /// Sets the initial conditions of the JFET.
        /// </summary>
        /// <param name="ic">The values.</param>
        [ParameterName("ic"), ParameterInfo("Initial VDS,VGS vector")]
        public void SetIc(double[] ic)
        {
            ic.ThrowIfNotLength(nameof(ic), 1, 2);
            switch (ic.Length)
            {
                case 2:
                    InitialVgs = ic[1];
                    goto case 1;
                case 1:
                    InitialVds = ic[0];
                    break;
            }
        }
    }
}
