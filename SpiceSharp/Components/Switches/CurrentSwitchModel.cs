using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a model for a current-controlled switch
    /// </summary>
    public class CurrentSwitchModel : CircuitModel<CurrentSwitchModel>
    {
        /// <summary>
        /// Register our parameters
        /// </summary>
        static CurrentSwitchModel()
        {
            Register();
        }

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
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the model</param>
        public CurrentSwitchModel(string name) : base(name)
        {
            // CurrentSwitch has a priority of -1, so this needs to be even earlier
            Priority = -2;
        }

        /// <summary>
        /// Setup the model
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
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
        }
    }
}
