using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Simulations.IntegrationMethods.Timesteps
{
    /// <summary>
    /// A class capable of managing timesteps and time with breakpoints 
    /// </summary>
    public class BreakTimesteps : Timesteps
    {
        /// <summary>
        /// Gets the breakpoints
        /// </summary>
        public Breakpoints Breaks { get; } = new Breakpoints();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="history"></param>
        protected BreakTimesteps(int history)
            : base(history)
        {
        }

        /// <summary>
        /// Probe a new time point
        /// </summary>
        /// <param name="delta">Timestep</param>
        public override void Probe(double delta)
        {
            // Do not allow going over the breakpoint
            if (SaveTime + delta >= Breaks.First)
                delta = Breaks.First - SaveTime;
            base.Probe(delta);
        }

        /// <summary>
        /// Accept the new time point
        /// </summary>
        public override void Accept()
        {
            base.Accept();

            // Clear the current breakpoint if necessary
            if (Time >= Breaks.First)
                Breaks.ClearBreakpoint();

            // Cut the delta if it extends beyond the next breakpoint
            if (Time + Delta >= Breaks.First)
                Delta = Time - Breaks.First;
        }
    }
}
