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
        [SpiceName("ron"), SpiceInfo("Closed resistance")]
        public Parameter CSWon { get; } = new Parameter(1.0);
        [SpiceName("roff"), SpiceInfo("Open resistance")]
        public Parameter CSWoff { get; } = new Parameter(1.0e12);
        [SpiceName("it"), SpiceInfo("Threshold current")]
        public Parameter CSWthresh { get; } = new Parameter();
        [SpiceName("ih"), SpiceInfo("Hysteresis current")]
        public Parameter CSWhyst { get; } = new Parameter();
        [SpiceName("gon"), SpiceInfo("Closed conductance")]
        public double CSWonConduct { get; private set; }
        [SpiceName("goff"), SpiceInfo("Open conductance")]
        public double CSWoffConduct { get; private set; }
    }
}
