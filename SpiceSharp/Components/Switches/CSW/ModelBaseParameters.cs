using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CSW
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentSwitchModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyNameAttribute("ron"), PropertyInfoAttribute("Closed resistance")]
        public Parameter CSWon { get; } = new Parameter(1.0);
        [PropertyNameAttribute("roff"), PropertyInfoAttribute("Open resistance")]
        public Parameter CSWoff { get; } = new Parameter(1.0e12);
        [PropertyNameAttribute("it"), PropertyInfoAttribute("Threshold current")]
        public Parameter CSWthresh { get; } = new Parameter();
        [PropertyNameAttribute("ih"), PropertyInfoAttribute("Hysteresis current")]
        public Parameter CSWhyst { get; } = new Parameter();
        [PropertyNameAttribute("gon"), PropertyInfoAttribute("Closed conductance")]
        public double CSWonConduct { get; private set; }
        [PropertyNameAttribute("goff"), PropertyInfoAttribute("Open conductance")]
        public double CSWoffConduct { get; private set; }
    }
}
