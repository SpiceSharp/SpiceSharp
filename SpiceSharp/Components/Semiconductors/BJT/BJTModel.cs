using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Components.ComponentBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="BJT"/>
    /// </summary>
    public class BJTModel : CircuitModel
    {
        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("npn"), SpiceInfo("NPN type device")]
        public void SetNPN(bool value)
        {
            if (value)
                BJTtype = NPN;
        }
        [SpiceName("pnp"), SpiceInfo("PNP type device")]
        public void SetPNP(bool value)
        {
            if (value)
                BJTtype = PNP;
        }
        [SpiceName("type"), SpiceInfo("NPN or PNP")]
        public string GetTYPE(Circuit ckt)
        {
            if (BJTtype == NPN)
                return "npn";
            else
                return "pnp";
        }
        public double BJTtype { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int NPN = 1;
        public const int PNP = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BJTModel(CircuitIdentifier name) : base(name)
        {
            RegisterBehavior(new BJTModelTemperatureBehavior());
        }
    }
}
