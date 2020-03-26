using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the temperature in degrees celsius.
        /// </summary>
        [ParameterName("temp"), ParameterInfo("Instance temperature", Units = "\u00b0C")]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the temperature in Kelvin.
        /// </summary>
        public GivenParameter<double> Temperature { get; set; } = new GivenParameter<double>(300.15, false);

        /// <summary>
        /// Gets the area.
        /// </summary>
        [ParameterName("area"), ParameterInfo("Area factor", Units = "m^2")]
        public double Area { get; set; } = 1;

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
        /// <param name="values">The values.</param>
        [ParameterName("ic"), ParameterInfo("Initial VDS,VGS vector")]
        public void SetIc(double[] values)
        {
            values.ThrowIfNull(nameof(values));
            switch (values.Length)
            {
                case 2:
                    InitialVgs = values[1];
                    goto case 1;
                case 1:
                    InitialVds = values[0];
                    break;
                default:
                    throw new BadParameterException(nameof(values));
            }
        }
    }
}
