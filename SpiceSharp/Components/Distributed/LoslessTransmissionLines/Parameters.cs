using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.LosslessTransmissionLines
{
    /// <summary>
    /// Base parameters for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public class Parameters : ParameterSet
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
        /// <value>
        /// The characteristic impedance.
        /// </value>
        [ParameterName("z0"), ParameterName("zo"), ParameterInfo("Characteristic impedance", Units = "\u03a9")]
        [GreaterThan(0)]
        public double Impedance
        {
            get => _impedance;
            set
            {
                Utility.GreaterThan(value, nameof(Impedance), 0);
                _impedance = value;
            }
        }

        /// <summary>
        /// Gets the frequency specification of the transmission line.
        /// </summary>
        /// <value>
        /// The frequency specification.
        /// </value>
        [ParameterName("f"), ParameterInfo("Frequency", Units = "Hz")]
        [GreaterThan(0)]
        public double Frequency
        {
            get => _frequency;
            set
            {
                Utility.GreaterThan(value, nameof(Frequency), 0);
                _frequency = value;
            }
        }

        /// <summary>
        /// Gets or sets the transmission delay of the transmission line.
        /// </summary>
        /// <value>
        /// The delay.
        /// </value>
        [ParameterName("td"), ParameterInfo("Transmission delay", Units = "s")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> Delay
        {
            get => _delay;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Delay), 0);
                _delay = value;
            }
        }

        /// <summary>
        /// Gets normalized length at the given <see cref="Frequency"/>.
        /// </summary>
        /// <value>
        /// The normalized length.
        /// </value>
        [ParameterName("nl"), ParameterInfo("Normalized length at the given frequency")]
        [GreaterThanOrEquals(0)]
        public double NormalizedLength
        {
            get => _normalizedLength;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(NormalizedLength), 0);
                _normalizedLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the relative tolerance used to determine if a breakpoint (where harsh nonlinear behavior occurs) needs to be added.
        /// </summary>
        /// <value>
        /// The relative tolerance.
        /// </value>
        [ParameterName("reltol"), ParameterInfo("The relative tolerance used to decide on adding a breakpoint.")]
        [GreaterThan(0)]
        public double RelativeTolerance
        {
            get => _relativeTolerance;
            set
            {
                Utility.GreaterThan(value, nameof(RelativeTolerance), 0);
                _relativeTolerance = value;
            }
        }

        /// <summary>
        /// Gets or sets the relative tolerance used to determine if a breakpoint (where harsh nonlinear behavior occurs) needs to be added.
        /// </summary>
        /// <value>
        /// The absolute tolerance.
        /// </value>
        [ParameterName("abstol"), ParameterInfo("The absolute tolerance used to decide on adding a breakpoint.")]
        [GreaterThan(0)]
        public double AbsoluteTolerance
        {
            get => _absoluteTolerance;
            set
            {
                Utility.GreaterThan(value, nameof(AbsoluteTolerance), 0);
                _absoluteTolerance = value;
            }
        }

        /// <summary>
        /// Gets the admittance (reciprocal of the impedance).
        /// </summary>
        /// <value>
        /// The admittance.
        /// </value>
        public double Admittance { get; private set; }

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        public void CalculateDefaults()
        {
            if (!Delay.Given)
                Delay = new GivenParameter<double>(NormalizedLength / Frequency, false);
            Admittance = 1.0 / Impedance;
        }
    }
}
