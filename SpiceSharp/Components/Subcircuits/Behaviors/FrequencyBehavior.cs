using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IFrequencyBehavior" />
    public class FrequencyBehavior : SubcircuitBehavior<IFrequencyBehavior>, IFrequencyBehavior
    {
        private bool _parallelInitialize, _parallelLoading;
        private ISolverElementProvider[] _solvers;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public FrequencyBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            // Figure out if we need to compute something in parallel
            _solvers = null;
            if (Simulations.Length > 1 &&
                context.Behaviors.Parameters.TryGetValue<FrequencyParameters>(out var fp))
            {
                _parallelInitialize = fp.ParallelInitialize;
                _parallelLoading = fp.ParallelLoad;

                if (_parallelLoading)
                {
                    _solvers = new ISolverElementProvider[Simulations.Length];
                    for (var i = 0; i < Simulations.Length; i++)
                    {
                        var state = Simulations[i].States.GetValue<IComplexSimulationState>();
                        _solvers[i] = (ISolverElementProvider)state.Solver;
                    }
                }
            }
            else
            {
                _parallelInitialize = false;
                _parallelLoading = false;
            }
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        public void InitializeParameters()
        {
            if (_parallelInitialize && Behaviors.Length > 1)
            {
                // Execute multiple tasks
                var tasks = new Task[Behaviors.Length];
                for (var t = 0; t < tasks.Length; t++)
                {
                    var bs = Behaviors[t];
                    tasks[t] = Task.Run(() =>
                    {
                        for (var i = 0; i < bs.Count; i++)
                            bs[i].InitializeParameters();
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
                        bs[i].InitializeParameters();
                }
            }
        }

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        public void Load()
        {
            if (_parallelLoading && Behaviors.Length > 1)
            {
                // Execute multiple tasks
                var tasks = new Task[Behaviors.Length];
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var bs = Behaviors[t];
                    tasks[t] = Task.Run(() =>
                    {
                        _solvers[t].Reset();
                        for (var i = 0; i < bs.Count; i++)
                            bs[i].Load();
                    });
                }
                Task.WaitAll(tasks);
                foreach (var solver in _solvers)
                    solver.ApplyElements();
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
        }
    }
}
