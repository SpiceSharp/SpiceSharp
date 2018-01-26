using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentSwitchBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentSwitchModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("ron"), PropertyInfo("Closed resistance")]
        public Parameter CSWon { get; } = new Parameter(1.0);
        [PropertyName("roff"), PropertyInfo("Open resistance")]
        public Parameter CSWoff { get; } = new Parameter(1.0e12);
        [PropertyName("it"), PropertyInfo("Threshold current")]
        public Parameter CSWthresh { get; } = new Parameter();
        [PropertyName("ih"), PropertyInfo("Hysteresis current")]
        public Parameter CSWhyst { get; } = new Parameter();
        [PropertyName("gon"), PropertyInfo("Closed conductance")]
        public double CSWonConduct { get; private set; }
        [PropertyName("goff"), PropertyInfo("Open conductance")]
        public double CSWoffConduct { get; private set; }
    }
}
