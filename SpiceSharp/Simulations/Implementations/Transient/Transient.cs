using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class implements the transient analysis.
    /// </summary>
    public class Transient : TimeSimulation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Transient"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        public Transient(string name) : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transient"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <param name="step">The step size.</param>
        /// <param name="final">The final time.</param>
        public Transient(string name, double step, double final) 
            : base(name, step, final)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transient"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="method">The method.</param>
        public Transient(string name, IIntegrationMethodDescription method)
            : base(name, method)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transient"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <param name="step">The step size.</param>
        /// <param name="final">The final time.</param>
        /// <param name="maxStep">The maximum step.</param>
        public Transient(string name, double step, double final, double maxStep) 
            : base(name, step, final, maxStep)
        {
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        protected override void Execute()
        {
            // First do temperature-dependent calculations and IC
            base.Execute();
            var exportargs = new ExportDataEventArgs(this);
            var config = Configurations.GetValue<IIntegrationMethodDescription>().ThrowIfNull("time configuration");

            // Start our statistics
            Statistics.TransientTime.Start();
            var stats = ((BiasingSimulation)this).Statistics;
            var startIters = stats.Iterations;
            var startselapsed = stats.SolveTime.Elapsed;

            try
            {
                while (true)
                {
                    // Accept the last evaluated time point
                    Accept();

                    // Export the current timepoint
                    if (Method.Time >= config.StartTime)
                        OnExport(exportargs);

                    // Detect the end of the simulation
                    if (Method.Time >= config.StopTime)
                    {
                        // Keep our statistics
                        Statistics.TransientTime.Stop();
                        Statistics.TransientIterations += stats.Iterations - startIters;
                        Statistics.TransientSolveTime += stats.SolveTime.Elapsed - startselapsed;

                        // Finished!
                        return;
                    }

                    // Continue integration
                    Method.Prepare();

                    // Find a valid time point
                    while (true)
                    {
                        // Probe the next time point
                        Probe();

                        // Try to solve the new point
                        var converged = TimeIterate(config.TranMaxIterations);
                        Statistics.TimePoints++;

                        // Did we fail to converge to a solution?
                        if (!converged)
                        {
                            Method.Reject();
                            Statistics.Rejected++;
                        }
                        else
                        {
                            // If our integration method approves of our solution, continue to the next timepoint
                            if (Method.Evaluate())
                                break;
                            Statistics.Rejected++;
                        }
                    }
                }
            }
            catch (SpiceSharpException ex)
            {
                // Keep our statistics
                Statistics.TransientTime.Stop();
                Statistics.TransientIterations += stats.Iterations - startIters;
                Statistics.TransientSolveTime += stats.SolveTime.Elapsed - startselapsed;
                throw new SpiceSharpException(Properties.Resources.Simulations_Time_Terminated.FormatString(Name), ex);
            }
        }
    }
}
