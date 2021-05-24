using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Inductors
{
    /// <summary>
    /// Base parameters for a <see cref="Inductor" />
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class Parameters : ParameterSet<Parameters>
    {
        /// <summary>
        /// Gets the inductance parameter.
        /// </summary>
        /// <value>
        /// The inductance.
        /// </value>
        [ParameterName("inductance"), ParameterInfo("Inductance of the inductor", Units = "H", IsPrincipal = true)]
        [GreaterThanOrEquals(0), Finite]
        private double _inductance;

        /// <summary>
        /// Gets the initial current parameter.
        /// </summary>
        /// <value>
        /// The initial current.
        /// </value>
        [ParameterName("ic"), ParameterInfo("Initial current through the inductor", Units = "V", Interesting = false)]
        [Finite]
        private GivenParameter<double> _initialCondition;

        /// <summary>
        /// Gets or sets the parallel multiplier.
        /// </summary>
        /// <value>
        /// The parallel multiplier.
        /// </value>
        [ParameterName("m"), ParameterInfo("Parallel multiplier")]
        [GreaterThan(0), Finite]
        private double _parallelMultiplier = 1.0;

        /// <summary>
        /// Gets or sets the series multiplier.
        /// </summary>
        /// <value>
        /// The series multiplier.
        /// </value>
        [ParameterName("n"), ParameterInfo("Series multiplier")]
        [GreaterThanOrEquals(0), Finite]
        private double _seriesMultiplier = 1.0;
    }
}
