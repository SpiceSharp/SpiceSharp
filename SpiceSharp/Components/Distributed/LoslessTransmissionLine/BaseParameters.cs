using SpiceSharp.Attributes;

namespace SpiceSharp.Components.LosslessTransmissionLineBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        [ParameterName("z0"), ParameterName("zo"), ParameterInfo("Characteristic impedance")]
        public GivenParameter<double> Impedance { get; } = new GivenParameter<double>();

        [ParameterName("f"), ParameterInfo("Frequency")]
        public GivenParameter<double> Frequency { get; } = new GivenParameter<double>(1e9);

        [ParameterName("td"), ParameterInfo("Transmission delay")]
        public GivenParameter<double> Delay { get; } = new GivenParameter<double>();

        [ParameterName("nl"), ParameterInfo("Normalized length at the given frequency")]
        public GivenParameter<double> NormalizedLength { get; } = new GivenParameter<double>(0.25);

        public double RelativeTolerance { get; set; }

        public double AbsoluteTolerance { get; set; }

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
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public override void CalculateDefaults()
        {
            if (!Delay.Given)
                Delay.RawValue = NormalizedLength.Value / Frequency.Value;

            if (!Impedance.Given)
                throw new CircuitException("{0}: Characteristic impedance is required.");
            Admittance = 1.0 / Impedance;
        }
    }
}
