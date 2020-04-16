using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Parameters for a <see cref="Resistor" />.
    /// </summary>
    public class Parameters : ParameterSet
    {
        /// <summary>
        /// The minimum resistance for any resistor.
        /// </summary>
        public const double MinimumResistance = 1e-3;

        /// <summary>
        /// Gets the temperature parameter in degrees Kelvin.
        /// </summary>
        public GivenParameter<double> Temperature { get; set; } = new GivenParameter<double>(Constants.ReferenceTemperature);

        /// <summary>
		/// Resistance
		/// </summary>
		[ParameterName("resistance"), ParameterName("r"), ParameterInfo("Resistance", Units = "\u03a9", IsPrincipal = true)]
		public GivenParameter<double> Resistance
		{
			get => _resistance;
			set
			{
				if (value < MinimumResistance)
				{
					_resistance = MinimumResistance;
					SpiceSharpWarning.Warning(this, Properties.Resources.Parameters_TooSmallSet.FormatString("resistance", "r", value, MinimumResistance));
					return;
				}
				_resistance = value;
			}
		}
		private GivenParameter<double> _resistance;

		/// <summary>
		/// Instance operating temperature
		/// </summary>
		[ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance operating temperature", Units = "\u00b0C", Interesting = false)]
		public double TemperatureCelsius
		{
			get => Temperature - Constants.CelsiusKelvin;
			set => Temperature = value + Constants.CelsiusKelvin;
		}

		/// <summary>
		/// Width
		/// </summary>
		[ParameterName("w"), ParameterInfo("Width", Units = "m")]
		public GivenParameter<double> Width
		{
			get => _width;
			set
			{
				if (value <= 0.0)
				{
					_width = 0.0;
					SpiceSharpWarning.Warning(this, Properties.Resources.Parameters_TooSmallSet.FormatString("w", value, 0.0));
					return;
				}
				_width = value;
			}
		}
		private GivenParameter<double> _width;

		/// <summary>
		/// Length
		/// </summary>
		[ParameterName("l"), ParameterInfo("Length", Units = "m")]
		public GivenParameter<double> Length
		{
			get => _length;
			set
			{
				if (value <= 0.0)
				{
					_length = 0.0;
					SpiceSharpWarning.Warning(this, Properties.Resources.Parameters_TooSmallSet.FormatString("l", value, 0.0));
					return;
				}
				_length = value;
			}
		}
		private GivenParameter<double> _length;

		/// <summary>
		/// Parallel multiplier
		/// </summary>
		[ParameterName("m"), ParameterInfo("Parallel multiplier")]
		public double ParallelMultiplier
		{
			get => _parallelMultiplier;
			set
			{
				if (value < 0.0)
					throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString("m", value, 0.0));
				_parallelMultiplier = value;
			}
		}
		private double _parallelMultiplier = 1.0;

		/// <summary>
		/// Series multiplier
		/// </summary>
		[ParameterName("n"), ParameterInfo("Series multiplier")]
		public double SeriesMultiplier
		{
			get => _seriesMultiplier;
			set
			{
				if (value <= 0.0)
					throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString("n", value, 0.0));
				_seriesMultiplier = value;
			}
		}
		private double _seriesMultiplier = 1.0;
    }
}