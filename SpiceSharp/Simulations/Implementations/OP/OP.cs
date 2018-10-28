namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class that implements the operating point analysis.
    /// </summary>
    /// <seealso cref="BaseSimulation" />
    public class OP : BaseSimulation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OP"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        public OP(string name) : base(name)
        {
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        protected override void Execute()
        {
            base.Execute();

            // Setup the state
            var state = RealState;
            state.UseIc = false; // UseIC is only used in transient simulations
            state.UseDc = true;

            Op(DcMaxIterations);

            var exportargs = new ExportDataEventArgs(this);
            OnExport(exportargs);
        }
    }
}
