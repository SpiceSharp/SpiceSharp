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
        [ParameterName("inductance"), PropertyInfo("Inductance of the inductor", IsPrincipal = true)]
        public Parameter Inductance { get; } = new Parameter();
        [ParameterName("ic"), PropertyInfo("Initial current through the inductor", Interesting = false)]
        public Parameter InitialCondition { get; } = new Parameter();

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
            Inductance.Set(inductance);
        }
    }
}
