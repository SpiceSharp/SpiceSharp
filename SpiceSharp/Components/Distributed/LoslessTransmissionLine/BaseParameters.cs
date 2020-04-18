using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.LosslessTransmissionLineBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public class BaseParameters : ParameterSet
    {
        private double _absoluteTolerance = 1.0;
        private double _relativeTolerance = 1.0;
        private double _normalizedLength = 0.25;
        private GivenParameter<double> _delay;
        private double _frequency = 1.0e9;
        private double _impedance = 50.0;

        /// <summary>
        /// Gets or sets the characteristic impedance.
        /// </summary>
        [ParameterName("z0"), ParameterName("zo"), ParameterInfo("Characteristic impedance", Units = "\u03a9")]
        [GreaterThan(0)]
        public double Impedance
        {
            get => _impedance;
            set
            {
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(Impedance), value, 0));
                _impedance = value;
            }
        }

        /// <summary>
        /// Gets the frequency parameter of the transmission line.
        /// </summary>
        [ParameterName("f"), ParameterInfo("Frequency", Units = "Hz")]
        [GreaterThan(0)]
        public double Frequency
        {
            get => _frequency;
            set
            {
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(Frequency), value, 0));
                _frequency = value;
            }
        }

        /// <summary>
        /// Gets or sets the transmission delay of the transmission line.
        /// </summary>
        [ParameterName("td"), ParameterInfo("Transmission delay", Units = "s")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> Delay
        {
            get => _delay;
            set
            {
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(Delay), value, 0));
                _delay = value;
            }
        }

        /// <summary>
        /// Gets normalized length parameter at the given frequency.
        /// </summary>
        [ParameterName("nl"), ParameterInfo("Normalized length at the given frequency")]
        [GreaterThanOrEquals(0)]
        public double NormalizedLength
        {
            get => _normalizedLength;
            set
            {
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(NormalizedLength), value, 0));
                _normalizedLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the relative tolerance used to determine if a breakpoint (where harsh nonlinear behavior occurs) needs to be added.
        /// </summary>
        [ParameterName("reltol"), ParameterInfo("The relative tolerance used to decide on adding a breakpoint.")]
        [GreaterThan(0)]
        public double RelativeTolerance
        {
            get => _relativeTolerance;
            set
            {
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(RelativeTolerance), value, 0));
                _relativeTolerance = value;
            }
        }

        /// <summary>
        /// Gets or sets the relative tolerance used to determine if a breakpoint (where harsh nonlinear behavior occurs) needs to be added.
        /// </summary>
        [ParameterName("abstol"), ParameterInfo("The absolute tolerance used to decide on adding a breakpoint.")]
        [GreaterThan(0)]
        public double AbsoluteTolerance
        {
            get => _absoluteTolerance;
            set
            {
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(AbsoluteTolerance), value, 0));
                _absoluteTolerance = value;
            }
        }

        /// <summary>
        /// Gets the admittance (reciprocal of the impedance).
        /// </summary>
        public double Admittance { get; private set; }

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        public override void CalculateDefaults()
        {
            if (!Delay.Given)
                Delay = NormalizedLength / Frequency;
            Admittance = 1.0 / Impedance;
        }
    }
}
