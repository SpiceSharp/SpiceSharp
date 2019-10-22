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
        private SubcircuitBiasingState[] _states;
        private HashSet<Variable> _common = new HashSet<Variable>();

        /// <summary>
        /// Gets the common variables.
        /// </summary>
        /// <value>
        /// The common variables.
        /// </value>
        public IEnumerable<Variable> CommonVariables => _common;

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

                _states = new SubcircuitBiasingState[Simulations.Length];
                for (var i = 0; i < Simulations.Length; i++)
                {
                    var state = Simulations[i].States.GetValue<IBiasingSimulationState>();
                    _states[i] = (SubcircuitBiasingState)state;
                    state.Setup(Simulations[i]);
                }
            }
            else
            {
                _parallelIsConvergent = false;
                _parallelLoading = false;
                _states = null;
            }

            // Get the common variables if there are more entity groups
            _common.Clear();
            if (Simulations.Length > 1)
            {
                var all = new HashSet<Variable>();
                foreach (var sim in Simulations)
                {
                    foreach (var variable in sim.Variables)
                    {
                        if (all.Contains(variable))
                            _common.Add(variable);
                        else
                            all.Add(variable);
                    }
                }

                // Add our own nodes as well
                var c = (ComponentBindingContext)context;
                for (var i = 0; i < c.Nodes.Length; i++)
                    _common.Add(c.Nodes[i]);

                // Notify the states that they should share these variables with other states
                foreach (var state in _states)
                    state.ShareVariables(_common);
            }
        }

        /// <summary>
        /// Destroy the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            if (_states != null)
            {
                for (var i = 0; i < _states.Length; i++)
                {
                    var state = (IBiasingSimulationState)_states[i];
                    state.Unsetup();
                }
                _states = null;
            }
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
                    var state = _states[t];
                    tasks[t] = Task.Run(() =>
                    {
                        var result = true;
                        for (var i = 0; i < bs.Count; i++)
                            result &= bs[i].IsConvergent();
                        if (result)
                            result &= state.CheckConvergence();
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
                if (_states != null)
                {
                    foreach (var state in _states)
                        isConvergent &= state.CheckConvergence();
                }
                return isConvergent;
            }
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
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
                    var state = _states[t];
                    tasks[t] = Task.Run(() =>
                    {
                        state.Reset();
                        for (var i = 0; i < bs.Count; i++)
                            bs[i].Load();
                        state.ApplyAsynchroneously();
                    });
                }
                Task.WaitAll(tasks);
                foreach (var state in _states)
                    state.ApplySynchroneously();
            }
            else
            {
                if (_states != null)
                {
                    for (var t = 0; t < Behaviors.Length; t++)
                    {
                        var bs = Behaviors[t];
                        _states[t].Reset();
                        for (var i = 0; i < bs.Count; i++)
                            bs[i].Load();
                        _states[t].ApplyAsynchroneously();
                    }
                    foreach (var state in _states)
                        state.ApplySynchroneously();
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
}
