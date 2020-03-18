using SpiceSharp.Attributes;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="Inductor"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the inductance parameter.
        /// </summary>
        [ParameterName("inductance"), ParameterInfo("Inductance of the inductor", IsPrincipal = true)]
        public GivenParameter<double> Inductance { get; set; }

        /// <summary>
        /// Gets the initial current parameter.
        /// </summary>
        [ParameterName("ic"), ParameterInfo("Initial current through the inductor", Interesting = false)]
        public GivenParameter<double> InitialCondition { get; set; }

        /// <summary>
        /// Gets or sets the parallel multiplier.
        /// </summary>
        [ParameterName("m"), ParameterInfo("Parallel multiplier")]
        public double ParallelMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the series multiplier.
        /// </summary>
        /// <value>
        /// The series multiplier.
        /// </value>
        [ParameterName("n"), ParameterInfo("Series multiplier")]
        public double SeriesMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        /// <param name="inductance">Inductor</param>
        public BaseParameters(double inductance)
        {
            Inductance = inductance;
        }
    }
}
