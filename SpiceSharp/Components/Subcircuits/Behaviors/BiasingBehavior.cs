using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="Subcircuit"/>
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IBiasingBehavior" />
    public class BiasingBehavior : SubcircuitBehavior<IBiasingBehavior>, IBiasingBehavior
    {
        private bool _parallelIsConvergent, _parallelLoading;
        private ISolverElementProvider[] _solvers;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public BiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            // Figure out if we need to compute something in parallel
            if (Simulations.Length > 1 &&
                context.Behaviors.Parameters.TryGetValue<BiasingParameters>(out var bp))
            {
                _parallelIsConvergent = bp.ParallelConvergences;
                _parallelLoading = bp.ParallelLoad;
            }
            else
            {
                _parallelIsConvergent = false;
                _parallelLoading = false;
            }

            // Deal with solvers
            var solvers = new List<ISolverElementProvider>(Simulations.Length);
            foreach (var simulation in Simulations)
            {
                var state = simulation.States.GetValue<IBiasingSimulationState>();
                if (state.Solver is ISolverElementProvider solver)
                    solvers.Add(solver);
            }
            if (solvers.Count > 0)
                _solvers = solvers.ToArray();
            else
                _solvers = null;
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvergent()
        {
            if (_parallelIsConvergent && Behaviors.Length > 1)
            {
                var tasks = new Task<bool>[Behaviors.Length];
                for (var t = 0; t < tasks.Length; t++)
                {
                    var bs = Behaviors[t];
                    tasks[t] = Task.Run(() =>
                    {
                        var result = true;
                        for (var i = 0; i < bs.Count; i++)
                            result &= bs[i].IsConvergent();
                        return result;
                    });
                }
                Task.WaitAll(tasks);
                var isConvergent = true;
                foreach (var task in tasks)
                    isConvergent &= task.Result;
                return isConvergent;
            }
            else
            {
                var isConvergent = true;
                foreach (var bs in Behaviors)
                {
                    for (var i = 0; i < bs.Count; i++)
                        isConvergent &= bs[i].IsConvergent();
                }
                return isConvergent;
            }
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        public void Load()
        {
            // Reset local solvers
            if (_solvers != null)
            {
                foreach (var solver in _solvers)
                    solver.Reset();
            }

            if (_parallelLoading && Behaviors.Length > 1)
            {
                // Execute multiple tasks
                var tasks = new Task[Behaviors.Length];
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var bs = Behaviors[t];
                    tasks[t] = Task.Run(() =>
                    {
                        for (var i = 0; i < bs.Count; i++)
                            bs[i].Load();
                    });
                }
                Task.WaitAll(tasks);
            }
            else
            {
                // Use single thread
                foreach (var bs in Behaviors)
                {
                    for (var i = 0; i < bs.Count; i++)
                        bs[i].Load();
                }
            }

            // Apply local solvers
            if (_solvers != null)
            {
                foreach (var solver in _solvers)
                    solver.ApplyElements();
            }
        }
    }
}
