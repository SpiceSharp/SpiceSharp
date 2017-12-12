using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Load behavior for a <see cref="CurrentSwitchModel"/>
    /// </summary>
    public class CurrentSwitchModelLoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("ron"), SpiceInfo("Closed resistance")]
        public Parameter CSWon { get; } = new Parameter();
        [SpiceName("roff"), SpiceInfo("Open resistance")]
        public Parameter CSWoff { get; } = new Parameter();
        [SpiceName("it"), SpiceInfo("Threshold current")]
        public Parameter CSWthresh { get; } = new Parameter();
        [SpiceName("ih"), SpiceInfo("Hysteresis current")]
        public Parameter CSWhyst { get; } = new Parameter();
        [SpiceName("gon"), SpiceInfo("Closed conductance")]
        public double CSWonConduct { get; private set; }
        [SpiceName("goff"), SpiceInfo("Open conductance")]
        public double CSWoffConduct { get; private set; }

        /// <summary>
        /// Setup the behaviors
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            if (!CSWon.Given)
            {
                CSWon.Value = 1.0;
                CSWonConduct = 1.0;
            }
            else
                CSWonConduct = 1.0 / CSWon.Value;

            if (!CSWoff.Given)
            {
                CSWoffConduct = ckt.State.Gmin;
                CSWoff.Value = 1.0 / CSWoffConduct;
            }
            else
                CSWoffConduct = 1.0 / CSWoff.Value;
            return false;
        }

        /// <summary>
        /// Load behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            // Do nothing
        }
    }
}
