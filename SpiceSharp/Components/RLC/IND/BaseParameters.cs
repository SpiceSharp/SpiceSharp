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
        [PropertyName("inductance"), PropertyInfo("Inductance of the inductor", IsPrincipal = true)]
        public Parameter INDinduct { get; } = new Parameter();
        [PropertyName("ic"), PropertyInfo("Initial current through the inductor", Interesting = false)]
        public Parameter INDinitCond { get; } = new Parameter();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ind">Inductor</param>
        public BaseParameters(double ind)
        {
            INDinduct.Set(ind);
        }
    }
}
