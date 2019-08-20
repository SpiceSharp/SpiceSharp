using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CurrentSourceBehaviors
{
    /// <summary>
    /// Accept behavior for a <see cref="CurrentSource"/>
    /// </summary>
    public class AcceptBehavior : Behavior, IAcceptBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private CommonBehaviors.IndependentSourceParameters _bp;

        /// <summary>
        /// Creates a new instance of the <see cref="AcceptBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public AcceptBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">Data provider</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Get parameters
            _bp = context.GetParameterSet<CommonBehaviors.IndependentSourceParameters>();
        }

        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        void IAcceptBehavior.Probe()
        {
            _bp.Waveform?.Probe((TimeSimulation)Simulation);
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        void IAcceptBehavior.Accept()
        {
            _bp.Waveform?.Accept((TimeSimulation)Simulation);
        }
    }
}
