using SpiceSharp.Attributes;

namespace SpiceSharp.Components.LosslessTransmissionLineBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the characteristic impedance.
        /// </summary>
        [ParameterName("z0"), ParameterName("zo"), ParameterInfo("Characteristic impedance")]
        public double Impedance { get; set; } = 50.0;

        /// <summary>
        /// Gets the frequency parameter of the transmission line.
        /// </summary>
        [ParameterName("f"), ParameterInfo("Frequency")]
        public GivenParameter<double> Frequency { get; } = new GivenParameter<double>(1e9);

        /// <summary>
        /// Gets or sets the transmission delay of the transmission line.
        /// </summary>
        [ParameterName("td"), ParameterInfo("Transmission delay")]
        public GivenParameter<double> Delay { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets normalized length parameter at the given frequency.
        /// </summary>
        [ParameterName("nl"), ParameterInfo("Normalized length at the given frequency")]
        public GivenParameter<double> NormalizedLength { get; } = new GivenParameter<double>(0.25);

        /// <summary>
        /// Gets or sets the relative tolerance used to determine if a breakpoint (where harsh nonlinear behavior occurs) needs to be added.
        /// </summary>
        [ParameterName("reltol"), ParameterInfo("The relative tolerance used to decide on adding a breakpoint.")]
        public double RelativeTolerance { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the relative tolerance used to determine if a breakpoint (where harsh nonlinear behavior occurs) needs to be added.
        /// </summary>
        [ParameterName("abstol"), ParameterInfo("The absolute tolerance used to decide on adding a breakpoint.")]
        public double AbsoluteTolerance { get; set; } = 1.0;

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
                Delay.RawValue = NormalizedLength.Value / Frequency.Value;

            if (Delay < 0.0)
                throw new CircuitException("Non-causal delay {0:e3} detected. Delays should be larger than 0.".FormatString(Delay));
            if (RelativeTolerance <= 0.0)
                throw new CircuitException("Relative tolerance {0:e3} should be larger than 0.".FormatString(RelativeTolerance));
            if (AbsoluteTolerance <= 0.0)
                throw new CircuitException("Absolute tolerance {0:e3} should be larger than 0.".FormatString(AbsoluteTolerance));

            // Calculate the admittance for saving a division operation
            if (Impedance <= 0.0)
                throw new CircuitException("Invalid characteristic impedance of {0:e3}. Should be larger than 0.".FormatString(Impedance));
            Admittance = 1.0 / Impedance;
        }
    }
}
