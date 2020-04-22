using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="DiodeModel" />
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public class ModelBaseParameters : ParameterSet
    {
        private double _breakdownCurrent = 1e-3;
        private GivenParameter<double> _breakdownVoltage = new GivenParameter<double>(-1.0, false);
        private double _depletionCapCoefficient = 0.5;
        private double _saturationCurrentExp = 3;
        private double _activationEnergy = 1.11;
        private double _gradingCoefficient = 0.5;
        private double _junctionPotential = 1;
        private double _junctionCap;
        private double _transitTime;
        private double _emissionCoefficient = 1;
        private double _resistance;
        private GivenParameter<double> _nominalTemperature = new GivenParameter<double>(Constants.ReferenceTemperature, false);
        private double _saturationCurrent = 1e-14;

        /// <summary>
        /// Gets the saturation current parameter.
        /// </summary>
        [ParameterName("is"), ParameterInfo("Saturation current", Units = "A")]
        [GreaterThan(0)]
        public double SaturationCurrent
        {
            get => _saturationCurrent;
            set
            {
                Utility.GreaterThan(value, nameof(SaturationCurrent), 0);
                _saturationCurrent = value;
            }
        }

        /// <summary>
        /// Gets or sets the nominal temperature in degrees Celsius.
        /// </summary>
        [ParameterName("tnom"), DerivedProperty(), ParameterInfo("Parameter measurement temperature", Units = "\u00b0C")]
        [GreaterThan(Constants.CelsiusKelvin)]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the nominal temperature parameter in degrees Kelvin.
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
        /// Gets the ohmic resistance parameter.
        /// </summary>
        [ParameterName("rs"), ParameterInfo("Ohmic resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0)]
        public double Resistance
        {
            get => _resistance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Resistance), 0);
                _resistance = value;
            }
        }

        /// <summary>
        /// Gets the mission coefficient parameter.
        /// </summary>
        [ParameterName("n"), ParameterInfo("Emission Coefficient")]
        [GreaterThan(0)]
        public double EmissionCoefficient
        {
            get => _emissionCoefficient;
            set
            {
                Utility.GreaterThan(value, nameof(EmissionCoefficient), 0);
                _emissionCoefficient = value;
            }
        }

        /// <summary>
        /// Gets the transit time parameter.
        /// </summary>
        [ParameterName("tt"), ParameterInfo("Transit Time", Units = "s")]
        [GreaterThanOrEquals(0)]
        public double TransitTime
        {
            get => _transitTime;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(TransitTime), 0);
                _transitTime = value;
            }
        }

        /// <summary>
        /// Gets the junction capacitance parameter.
        /// </summary>
        [ParameterName("cjo"), ParameterName("cj0"), ParameterInfo("Junction capacitance", Units = "F")]
        [GreaterThanOrEquals(0)]
        public double JunctionCap
        {
            get => _junctionCap;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(JunctionCap), 0);
                _junctionCap = value;
            }
        }

        /// <summary>
        /// Gets the junction built-in potential parameter.
        /// </summary>
        [ParameterName("vj"), ParameterInfo("Junction potential", Units = "V")]
        [GreaterThan(0)]
        public double JunctionPotential
        {
            get => _junctionPotential;
            set
            {
                Utility.GreaterThan(value, nameof(JunctionPotential), 0);
                _junctionPotential = value;
            }
        }

        /// <summary>
        /// Gets the grading coefficient parameter.
        /// </summary>
        [ParameterName("m"), ParameterInfo("Grading coefficient")]
        [GreaterThan(0), UpperLimit(0.9)]
        public double GradingCoefficient
        {
            get => _gradingCoefficient;
            set
            {
                Utility.GreaterThan(value, nameof(GradingCoefficient), 0);
                value = Utility.UpperLimit(value, this, nameof(GradingCoefficient), 0.9);
                _gradingCoefficient = value;
            }
        }

        /// <summary>
        /// Gets the activation energy parameter.
        /// </summary>
        [ParameterName("eg"), ParameterInfo("Activation energy", Units = "eV")]
        [GreaterThan(0), LowerLimit(0.1)]
        public double ActivationEnergy
        {
            get => _activationEnergy;
            set
            {
                Utility.GreaterThan(value, nameof(ActivationEnergy), 0);
                value = Utility.LowerLimit(value, this, nameof(ActivationEnergy), 0.1);
                _activationEnergy = value;
            }
        }

        /// <summary>
        /// Gets the saturation current temperature exponent parameter.
        /// </summary>
        [ParameterName("xti"), ParameterInfo("Saturation current temperature exponent")]
        [GreaterThanOrEquals(0)]
        public double SaturationCurrentExp
        {
            get => _saturationCurrentExp;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(SaturationCurrentExp), 0);
                _saturationCurrentExp = value;
            }
        }

        /// <summary>
        /// Gets the forward bias junction fit parameter.
        /// </summary>
        [ParameterName("fc"), ParameterInfo("Forward bias junction fit parameter")]
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
        /// Gets or sets the reverse breakdown voltage parameter. When NaN, the breakdown voltage is ignored.
        /// </summary>
        /// <value>
        /// The breakdown voltage.
        /// </value>
        [ParameterName("bv"), ParameterInfo("Reverse breakdown voltage", Units = "V")]
        [LessThan(0)]
        public GivenParameter<double> BreakdownVoltage
        {
            get => _breakdownVoltage;
            set
            {
                Utility.LessThan(value, nameof(BreakdownVoltage), 0);
                _breakdownVoltage = value;
            }
        }

        /// <summary>
        /// Gets the current parameter at the reverse breakdown voltage.
        /// </summary>
        /// <value>
        /// The breakdown current.
        /// </value>
        [ParameterName("ibv"), ParameterInfo("Current at reverse breakdown voltage", Units = "A")]
        [GreaterThan(0)]
        public double BreakdownCurrent
        {
            get => _breakdownCurrent;
            set
            {
                Utility.GreaterThan(value, nameof(BreakdownCurrent), 0);
                _breakdownCurrent = value;
            }
        }
    }
}