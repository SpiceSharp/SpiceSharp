using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.VSW
{
    /// <summary>
    /// Accept behavior for a <see cref="Components.VoltageSwitch"/>
    /// </summary>
    public class AcceptBehavior : Behaviors.AcceptBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        LoadBehavior load;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcceptBehavior(Identifier name) : base (name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>();
        }

        /// <summary>
        /// Accept behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Accept(Circuit ckt)
        {
            // Flag the load behavior to use our old state
            load.VSWuseOldState = true;

            // Copy the current state to the old state
            load.VSWoldState = load.VSWcurrentState;
        }
    }
}
