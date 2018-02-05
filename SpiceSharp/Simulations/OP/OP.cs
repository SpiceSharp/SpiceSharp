using SpiceSharp.Circuits;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Operating Point analysis
    /// </summary>
    public class OP : BaseSimulation
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the simulation</param>
        public OP(Identifier name) : base(name)
        {
        }

        /// <summary>
        /// Execute the DC simulation
        /// </summary>
        protected override void Execute()
        {
            // Setup the state
            var state = RealState;
            var baseconfig = BaseConfiguration;
            state.UseIC = false; // UseIC is only used in transient simulations
            state.UseDC = true;
            state.Domain = RealState.DomainType.None;
            state.Gmin = baseconfig.Gmin;

            Op(baseconfig.DCMaxIterations);

            var exportargs = new ExportDataEventArgs(RealState);
            Export(exportargs);
        }
    }
}
