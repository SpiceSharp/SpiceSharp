using SpiceSharp.Attributes;

namespace SpiceSharp.Components.LosslessTransmissionLineBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
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
        public double Frequency { get; set; } = 1.0e9;

        /// <summary>
        /// Gets or sets the transmission delay of the transmission line.
        /// </summary>
        [ParameterName("td"), ParameterInfo("Transmission delay")]
        public GivenParameter<double> Delay { get; set; }

        /// <summary>
        /// Gets normalized length parameter at the given frequency.
        /// </summary>
        [ParameterName("nl"), ParameterInfo("Normalized length at the given frequency")]
        public double NormalizedLength { get; set; } = 0.25;

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
                Delay = NormalizedLength / Frequency;

            if (Delay < 0.0)
                throw new BadParameterException(nameof(Delay), Delay, 
                    Properties.Resources.Delays_NonCausalDelay);
            if (RelativeTolerance <= 0.0)
                throw new BadParameterException(nameof(RelativeTolerance), RelativeTolerance, 
                    Properties.Resources.Delays_RelativeToleranceTooSmall);
            if (AbsoluteTolerance <= 0.0)
                throw new BadParameterException(nameof(AbsoluteTolerance), AbsoluteTolerance, 
                    Properties.Resources.Delays_AbsoluteToleranceTooSmall);

            // Calculate the admittance for saving a division operation
            if (Impedance <= 0.0)
                throw new BadParameterException(nameof(Impedance), Impedance,
                    Properties.Resources.TransmissionLines_ImpedanceTooSmall);
            Admittance = 1.0 / Impedance;
        }
    }
}
