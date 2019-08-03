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
        public GivenParameter<double> Inductance { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the initial current parameter.
        /// </summary>
        [ParameterName("ic"), ParameterInfo("Initial current through the inductor", Interesting = false)]
        public GivenParameter<double> InitialCondition { get; } = new GivenParameter<double>();

        /// <summary>
        /// Creates a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        /// <param name="inductance">Inductor</param>
        public BaseParameters(double inductance)
        {
            Inductance.Value = inductance;
        }
    }
}
