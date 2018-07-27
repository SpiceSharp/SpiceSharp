using SpiceSharp.Attributes;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="Inductor"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("inductance"), ParameterInfo("Inductance of the inductor", IsPrincipal = true)]
        public GivenParameter<double> Inductance { get; } = new GivenParameter<double>();
        [ParameterName("ic"), ParameterInfo("Initial current through the inductor", Interesting = false)]
        public GivenParameter<double> InitialCondition { get; } = new GivenParameter<double>();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inductance">Inductor</param>
        public BaseParameters(double inductance)
        {
            Inductance.Value = inductance;
        }
    }
}
