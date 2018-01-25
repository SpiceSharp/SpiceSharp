using SpiceSharp.Attributes;

namespace SpiceSharp.Components.IND
{
    /// <summary>
    /// Base parameters for a <see cref="Inductor"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [NameAttribute("inductance"), InfoAttribute("Inductance of the inductor", IsPrincipal = true)]
        public Parameter INDinduct { get; } = new Parameter();
        [NameAttribute("ic"), InfoAttribute("Initial current through the inductor", Interesting = false)]
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
