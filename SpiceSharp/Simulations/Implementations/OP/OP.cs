namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class that implements the operating point analysis.
    /// </summary>
    /// <seealso cref="BiasingSimulation" />
    public class OP : BiasingSimulation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OP"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
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
            var state = BiasingState;
            state.UseIc = false; // UseIC is only used in transient simulations
            state.UseDc = true;

            Op(DcMaxIterations);

            var exportargs = new ExportDataEventArgs(this);
            OnExport(exportargs);
        }
    }
}
