using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;
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
        private BiasingParameters _bp;
        private BiasingSimulationState[] _states;

        /// <summary>
        /// Occurs when the Y-matrix and Rhs-vector has been loaded by the behavior.
        /// </summary>
        public event EventHandler<BiasingEventArgs> AfterLoad;

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
            _states = null;

            if (Simulations.Length > 1)
            {
                // Figure out if we need to compute something in parallel
                if (context.Behaviors.Parameters.TryGetValue(out _bp))
                {
                    _states = new BiasingSimulationState[Simulations.Length];
                    for (var i = 0; i < Simulations.Length; i++)
                    {
                        var state = Simulations[i].States.GetValue<IBiasingSimulationState>();
                        _states[i] = (BiasingSimulationState)state;
                        state.Setup(Simulations[i]);
                    }

                    // Get all shared variables between different tasks
                    var common = GetCommonVariables();
                    var c = (ComponentBindingContext)context;
                    for (var i = 0; i < c.Nodes.Length; i++)
                        common.Add(c.Nodes[i]);

                    // Notify the states that they should share these variables with other states
                    foreach (var state in _states)
                        state.ShareVariables(common);
                }
            }
        }

        /// <summary>
        /// Gets the common variables.
        /// </summary>
        /// <returns></returns>
        public HashSet<Variable> GetCommonVariables()
        {
            var common = new HashSet<Variable>();
            var all = new HashSet<Variable>();
            foreach (var sim in Simulations)
            {
                foreach (var variable in sim.Variables)
                {
                    if (all.Contains(variable))
                        common.Add(variable);
                    else
                        all.Add(variable);
                }
            }
            return common;
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
            if (_bp != null && _bp.ParallelConvergences && Behaviors.Length > 1)
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
            if (_bp != null && (_bp.ParallelLoad || _bp.ParallelSolve) && Behaviors.Length > 1)
            {
                // Execute multiple tasks
                var tasks = new Task[Behaviors.Length];
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var task = t;
                    tasks[t] = Task.Run(() =>
                    {
                        var state = _states[task];
                        do
                        {
                            state.Reset();
                            LoadSubcircuitBehaviors(task);
                        }
                        while (!state.ApplyAsynchroneously());
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
                        _states[t].Reset();
                        LoadSubcircuitBehaviors(t);
                        _states[t].ApplyAsynchroneously();
                    }
                    foreach (var state in _states)
                        state.ApplySynchroneously();
                }
                else
                {
                    // Use single thread
                    for (var t = 0; t < Behaviors.Length; t++)
                        LoadSubcircuitBehaviors(t);
                }
            }
        }

        /// <summary>
        /// Loads the subcircuit behaviors.
        /// </summary>
        /// <param name="task">The task.</param>
        private void LoadSubcircuitBehaviors(int task)
        {
            var bs = Behaviors[task];
            for (var i = 0; i < bs.Count; i++)
                bs[i].Load();
            AfterLoad?.Invoke(this, new BiasingEventArgs(task));
        }
    }
}
