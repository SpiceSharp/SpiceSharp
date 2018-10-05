using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class implements the transient analysis.
    /// </summary>
    public class Transient : TimeSimulation
    {
        /// <summary>
        /// Behaviors for accepting a timepoint
        /// </summary>
        private BehaviorList<BaseAcceptBehavior> _acceptBehaviors;

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
        /// Set up the simulation.
        /// </summary>
        /// <param name="circuit">The circuit that will be used.</param>
        /// <exception cref="ArgumentNullException">circuit</exception>
        protected override void Setup(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));
            base.Setup(circuit);

            // Get behaviors and configurations
            _acceptBehaviors = SetupBehaviors<BaseAcceptBehavior>(circuit.Entities);
        }

        /// <summary>
        /// Destroys the simulation.
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            for (var i = 0; i < _acceptBehaviors.Count; i++)
                _acceptBehaviors[i].Unsetup(this);
            _acceptBehaviors = null;

            base.Unsetup();
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        /// <exception cref="SpiceSharp.CircuitException">{0}: transient terminated".FormatString(Name)</exception>
        protected override void Execute()
        {
            // First do temperature-dependent calculations and IC
            base.Execute();
            var exportargs = new ExportDataEventArgs(this);
            var timeConfig = Configurations.Get<TimeConfiguration>();

            // Start our statistics
            Statistics.TransientTime.Start();
            var startIters = Statistics.Iterations;
            var startselapsed = Statistics.SolveTime.Elapsed;

            try
            {
                var newDelta = Math.Min(timeConfig.FinalTime / 50.0, timeConfig.Step) / 10.0;
                while (true)
                {
                    // Accept the last evaluated time point
                    for (var i = 0; i < _acceptBehaviors.Count; i++)
                        _acceptBehaviors[i].Accept(this);
                    Method.Accept(this);
                    Statistics.Accepted++;

                    // Export the current timepoint
                    if (Method.Time >= timeConfig.InitTime)
                        OnExport(exportargs);

                    // Detect the end of the simulation
                    if (Method.Time >= timeConfig.FinalTime)
                    {
                        // Keep our statistics
                        Statistics.TransientTime.Stop();
                        Statistics.TransientIterations += Statistics.Iterations - startIters;
                        Statistics.TransientSolveTime += Statistics.SolveTime.Elapsed - startselapsed;

                        // Finished!
                        return;
                    }

                    // Continue integration
                    Method.Continue(this, ref newDelta);

                    // Find a valid time point
                    while (true)
                    {
                        // Probe the next time point
                        Method.Probe(this, newDelta);

                        // Try to solve the new point
                        var converged = TimeIterate(timeConfig.TranMaxIterations);
                        Statistics.TimePoints++;

                        // Did we fail to converge to a solution?
                        if (!converged)
                        {
                            Method.NonConvergence(this, out newDelta);
                            Statistics.Rejected++;
                        }
                        else
                        {
                            // If our integration method approves of our solution, continue to the next timepoint
                            if (Method.Evaluate(this, out newDelta))
                                break;
                            Statistics.Rejected++;
                        }
                    }
                }
            }
            catch (CircuitException ex)
            {
                // Keep our statistics
                Statistics.TransientTime.Stop();
                Statistics.TransientIterations += Statistics.Iterations - startIters;
                Statistics.TransientSolveTime += Statistics.SolveTime.Elapsed - startselapsed;
                throw new CircuitException("{0}: transient terminated".FormatString(Name), ex);
            }
        }
    }
}
