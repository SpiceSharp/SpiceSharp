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
        /// <param name="name">The identifier of the simulation.</param>
        public Transient(string name) : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transient"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="step">The step size.</param>
        /// <param name="final">The final time.</param>
        public Transient(string name, double step, double final) 
            : base(name, step, final)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transient"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
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
            var timeConfig = Configurations.Get<TimeConfiguration>().ThrowIfNull("time configuration");

            // Start our statistics
            TimeSimulationStatistics.TransientTime.Start();
            var startIters = BaseSimulationStatistics.Iterations;
            var startselapsed = BaseSimulationStatistics.SolveTime.Elapsed;

            try
            {
                var newDelta = Math.Min(timeConfig.FinalTime / 50.0, timeConfig.Step) / 10.0;
                while (true)
                {
                    // Accept the last evaluated time point
                    Accept();

                    // Export the current timepoint
                    if (Method.Time >= timeConfig.InitTime)
                        OnExport(exportargs);

                    // Detect the end of the simulation
                    if (Method.Time >= timeConfig.FinalTime)
                    {
                        // Keep our statistics
                        TimeSimulationStatistics.TransientTime.Stop();
                        TimeSimulationStatistics.TransientIterations += BaseSimulationStatistics.Iterations - startIters;
                        TimeSimulationStatistics.TransientSolveTime += BaseSimulationStatistics.SolveTime.Elapsed - startselapsed;

                        // Finished!
                        return;
                    }

                    // Continue integration
                    Method.Continue(this, ref newDelta);

                    // Find a valid time point
                    while (true)
                    {
                        // Probe the next time point
                        Probe(newDelta);

                        // Try to solve the new point
                        var converged = TimeIterate(timeConfig.TranMaxIterations);
                        TimeSimulationStatistics.TimePoints++;

                        // Did we fail to converge to a solution?
                        if (!converged)
                        {
                            Method.NonConvergence(this, out newDelta);
                            TimeSimulationStatistics.Rejected++;
                        }
                        else
                        {
                            // If our integration method approves of our solution, continue to the next timepoint
                            if (Method.Evaluate(this, out newDelta))
                                break;
                            TimeSimulationStatistics.Rejected++;
                        }
                    }
                }
            }
            catch (CircuitException ex)
            {
                // Keep our statistics
                TimeSimulationStatistics.TransientTime.Stop();
                TimeSimulationStatistics.TransientIterations += BaseSimulationStatistics.Iterations - startIters;
                TimeSimulationStatistics.TransientSolveTime += BaseSimulationStatistics.SolveTime.Elapsed - startselapsed;
                throw new CircuitException("{0}: transient terminated".FormatString(Name), ex);
            }
        }
    }
}
