using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="JFETModel" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public class ModelBaseParameters : ParameterSet
    {
        private double _b = 1;
        private double _depletionCapCoefficient = 0.5;
        private double _gateSaturationCurrent = 1e-14;
        private double _gatePotential = 1;
        private double _capGd;
        private double _capGs;
        private double _sourceResistance;
        private double _drainResistance;
        private double _lModulation;
        private double _beta = 1e-4;
        private GivenParameter<double> _nominalTemperature = new GivenParameter<double>(300.15, false);
        private double _nominalTemperatureCelsius;

        /// <summary>
        /// Gets or sets the measurement temperature in degrees celsius.
        /// </summary>
        [ParameterName("tnom"), ParameterInfo("Nominal temperature.", Units = "\u00b0C")]
        [GreaterThan(Constants.CelsiusKelvin)]
        public double NominalTemperatureCelsius
        {
            get => _nominalTemperatureCelsius;
            set
            {
                Utility.GreaterThan(value, nameof(NominalTemperatureCelsius), Constants.CelsiusKelvin);
                _nominalTemperatureCelsius = value;
            }
        }

        /// <summary>
        /// Gets the measurement temperature in Kelvin.
        /// </summary>
        [GreaterThan(0)]
        public GivenParameter<double> NominalTemperature
        {
            get => _nominalTemperature;
            set
            {
                Utility.GreaterThan(value, nameof(NominalTemperature), 0);
                _nominalTemperature = value;
            }
        }

        /// <summary>
        /// Gets the threshold voltage.
        /// </summary>
        [ParameterName("vt0"), ParameterName("vto"), ParameterInfo("Threshold voltage", Units = "V")]
        public double Threshold { get; set; } = -2;

        /// <summary>
        /// Gets the transconductance.
        /// </summary>
        [ParameterName("beta"), ParameterInfo("Transconductance parameter", Units = "\u03a9^-1")]
        [GreaterThanOrEquals(0)]
        public double Beta
        {
            get => _beta;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Beta), 0);
                _beta = value;
            }
        }

        /// <summary>
        /// Gets the channel length modulation parameter.
        /// </summary>
        [ParameterName("lambda"), ParameterInfo("Channel length modulation parameter", Units = "V^-1")]
        [GreaterThanOrEquals(0)]
        public double LModulation
        {
            get => _lModulation;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(LModulation), 0);
                _lModulation = value;
            }
        }

        /// <summary>
        /// Gets the drain resistance.
        /// </summary>
        [ParameterName("rd"), ParameterInfo("Drain ohmic resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0)]
        public double DrainResistance
        {
            get => _drainResistance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(DrainResistance), 0);
                _drainResistance = value;
            }
        }

        /// <summary>
        /// Gets the source resistance.
        /// </summary>
        [ParameterName("rs"), ParameterInfo("Source ohmic resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0)]
        public double SourceResistance
        {
            get => _sourceResistance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(SourceResistance), 0);
                _sourceResistance = value;
            }
        }

        /// <summary>
        /// Gets the gate-source junction capacitance.
        /// </summary>
        [ParameterName("cgs"), ParameterInfo("G-S junction capacitance", Units = "F")]
        [GreaterThanOrEquals(0)]
        public double CapGs
        {
            get => _capGs;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(CapGs), 0);
                _capGs = value;
            }
        }

        /// <summary>
        /// Gets the gate-drain junction capacitance.
        /// </summary>
        [ParameterName("cgd"), ParameterInfo("G-D junction capacitance", Units = "F")]
        [GreaterThanOrEquals(0)]
        public double CapGd
        {
            get => _capGd;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(CapGd), 0);
                _capGd = value;
            }
        }

        /// <summary>
        /// Gets the gate junction potential.
        /// </summary>
        [ParameterName("pb"), ParameterInfo("Gate junction potential", Units = "V")]
        [GreaterThan(0)]
        public double GatePotential
        {
            get => _gatePotential;
            set
            {
                Utility.GreaterThan(value, nameof(GatePotential), 0);
                _gatePotential = value;
            }
        }

        /// <summary>
        /// Gets the gate saturation current.
        /// </summary>
        [ParameterName("is"), ParameterInfo("Gate junction saturation current", Units = "A")]
        [GreaterThan(0)]
        public double GateSaturationCurrent
        {
            get => _gateSaturationCurrent;
            set
            {
                Utility.GreaterThan(value, nameof(GateSaturationCurrent), 0);
                _gateSaturationCurrent = value;
            }
        }

        /// <summary>
        /// Gets the forward bias junction fitting parameter.
        /// </summary>
        [ParameterName("fc"), ParameterInfo("Forward bias junction fitting parameter")]
        [GreaterThan(0), UpperLimit(0.95)]
        public double DepletionCapCoefficient
        {
            get => _depletionCapCoefficient;
            set
            {
                Utility.GreaterThan(value, nameof(DepletionCapCoefficient), 0);
                value = Utility.UpperLimit(value, this, nameof(DepletionCapCoefficient), 0.95);
                _depletionCapCoefficient = value;
            }
        }

        /// <summary>
        /// Gets the doping tail parameter.
        /// </summary>
        [ParameterName("b"), ParameterInfo("Doping tail parameter")]
        [GreaterThanOrEquals(0)]
        public double B
        {
            get => _b;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(B), 0);
                _b = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the jfet.
        /// </summary>
        public double JFETType { get; protected set; } = 1.0;

        /// <summary>
        /// Gets the drain conductance.
        /// </summary>
        public double DrainConductance { get; private set; }

        /// <summary>
        /// Gets the source conductance.
        /// </summary>
        public double SourceConductance { get; private set; }

        /// <summary>
        /// Sets the model to be n-type.
        /// </summary>
        /// <param name="flag">if set to <c>true</c> n-type is set.</param>
        [ParameterName("njf"), ParameterInfo("N type JFET model")]
        public void SetNjf(bool flag)
        {
            if (flag)
                JFETType = 1.0;
        }

        /// <summary>
        /// Sets the model to be p-type.
        /// </summary>
        /// <param name="flag">if set to <c>true</c> p-type is set.</param>
        [ParameterName("pjf"), ParameterInfo("P type JFET model")]
        public void SetPjf(bool flag)
        {
            if (flag)
                JFETType = -1.0;
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        [ParameterName("type"), ParameterInfo("N-type or P-type JFET model")]
        public string TypeName
        {
            get
            {
                if (JFETType > 0.0)
                    return "njf";
                return "pjf";
            }
        }

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public override void CalculateDefaults()
        {
            DrainConductance = DrainResistance > 0 ? 1 / DrainResistance : 0;
            SourceConductance = SourceResistance > 0 ? 1 / SourceResistance : 0;
        }
    }
}
