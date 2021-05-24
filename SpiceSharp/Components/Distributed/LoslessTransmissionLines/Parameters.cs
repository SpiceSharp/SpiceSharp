using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.LosslessTransmissionLines
{
    /// <summary>
    /// Base parameters for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class Parameters : ParameterSet<Parameters>
    {
        /// <summary>
        /// Gets or sets the characteristic impedance.
        /// </summary>
        /// <value>
        /// The characteristic impedance.
        /// </value>
        [ParameterName("z0"), ParameterName("zo"), ParameterInfo("Characteristic impedance", Units = "\u03a9")]
        [GreaterThan(0), Finite]
        private double _impedance = 50.0;

        /// <summary>
        /// Gets the frequency specification of the transmission line.
        /// </summary>
        /// <value>
        /// The frequency specification.
        /// </value>
        [ParameterName("f"), ParameterInfo("Frequency", Units = "Hz")]
        [GreaterThan(0), Finite]
        private double _frequency = 1.0e9;

        /// <summary>
        /// Gets or sets the transmission delay of the transmission line.
        /// </summary>
        /// <value>
        /// The delay.
        /// </value>
        [ParameterName("td"), ParameterInfo("Transmission delay", Units = "s")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _delay;

        /// <summary>
        /// Gets normalized length at the given <see cref="Frequency"/>.
        /// </summary>
        /// <value>
        /// The normalized length.
        /// </value>
        [ParameterName("nl"), ParameterInfo("Normalized length at the given frequency")]
        [GreaterThanOrEquals(0), Finite]
        private double _normalizedLength = 0.25;

        /// <summary>
        /// Gets or sets the relative tolerance used to determine if a breakpoint (where harsh nonlinear behavior occurs) needs to be added.
        /// </summary>
        /// <value>
        /// The relative tolerance.
        /// </value>
        [ParameterName("reltol"), ParameterInfo("The relative tolerance used to decide on adding a breakpoint.")]
        [GreaterThan(0), Finite]
        private double _relativeTolerance = 1.0;

        /// <summary>
        /// Gets or sets the relative tolerance used to determine if a breakpoint (where harsh nonlinear behavior occurs) needs to be added.
        /// </summary>
        /// <value>
        /// The absolute tolerance.
        /// </value>
        [ParameterName("abstol"), ParameterInfo("The absolute tolerance used to decide on adding a breakpoint.")]
        [GreaterThan(0), Finite]
        private double _absoluteTolerance = 1.0;

        /// <summary>
        /// Gets the admittance (reciprocal of the impedance).
        /// </summary>
        /// <value>
        /// The admittance.
        /// </value>
        public double Admittance { get; private set; }

        /// <summary>
        /// Gets or sets the number of transmission lines in parallel.
        /// </summary>
        /// <value>
        /// The number of transmission lines in parallel.
        /// </value>
        [ParameterName("m"), ParameterInfo("Parallel multiplier")]
        [GreaterThanOrEquals(0)]
        private double _parallelMultiplier = 1.0;

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
