using SpiceSharp.Circuits;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Operating Point analysis
    /// </summary>
    public class OP : Simulation
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the simulation</param>
        /// <param name="config">Configuration</param>
        public OP(string name) : base(name)
        {
        }

        /// <summary>
        /// Execute the DC simulation
        /// </summary>
        protected override void Execute()
        {
            var ckt = Circuit;

            // Setup the state
            var state = ckt.State;
            var rstate = state;
            var config = CurrentConfig;
            state.UseIC = false; // UseIC is only used in transient simulations
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Domain = CircuitState.DomainTypes.None;
            state.Gmin = config.Gmin;

            Initialize(ckt);
            Op(ckt, config.DcMaxIterations);
            Export(ckt);
            Finalize(ckt);
        }
    }
}
