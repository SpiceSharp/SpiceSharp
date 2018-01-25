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
        [NameAttribute("ron"), InfoAttribute("Closed resistance")]
        public Parameter CSWon { get; } = new Parameter(1.0);
        [NameAttribute("roff"), InfoAttribute("Open resistance")]
        public Parameter CSWoff { get; } = new Parameter(1.0e12);
        [NameAttribute("it"), InfoAttribute("Threshold current")]
        public Parameter CSWthresh { get; } = new Parameter();
        [NameAttribute("ih"), InfoAttribute("Hysteresis current")]
        public Parameter CSWhyst { get; } = new Parameter();
        [NameAttribute("gon"), InfoAttribute("Closed conductance")]
        public double CSWonConduct { get; private set; }
        [NameAttribute("goff"), InfoAttribute("Open conductance")]
        public double CSWoffConduct { get; private set; }
    }
}
