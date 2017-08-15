using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class describes a model for a voltage-controlled switch
    /// </summary>
    public class VoltageSwitchModel : CircuitModel<VoltageSwitchModel>
    {
        /// <summary>
        /// Register our parameters
        /// </summary>
        static VoltageSwitchModel()
        {
            Register();
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("ron"), SpiceInfo("Resistance when closed")]
        public Parameter VSWon { get; } = new Parameter();
        [SpiceName("roff"), SpiceInfo("Resistance when off")]
        public Parameter VSWoff { get; } = new Parameter();
        [SpiceName("vt"), SpiceInfo("Threshold voltage")]
        public Parameter VSWthresh { get; } = new Parameter();
        [SpiceName("vh"), SpiceInfo("Hysteresis voltage")]
        public Parameter VSWhyst { get; } = new Parameter();
        [SpiceName("gon"), SpiceInfo("Conductance when closed")]
        public double VSWonConduct { get; private set; }
        [SpiceName("goff"), SpiceInfo("Conductance when closed")]
        public double VSWoffConduct { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the model</param>
        public VoltageSwitchModel(string name) : base(name) { }

        /// <summary>
        /// Setup the model
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            if (!VSWon.Given)
            {
                VSWonConduct = 1.0;
                VSWon.Value = 1.0;
            }
            else
                VSWonConduct = 1.0 / VSWon.Value;

            if (!VSWoff.Given)
            {
                VSWoffConduct = ckt.State.Gmin;
                VSWoff.Value = 1.0 / VSWoffConduct;
            }
            else
                VSWoffConduct = 1.0 / VSWoff.Value;
        }
    }
}
