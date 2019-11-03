using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IFrequencyBehavior" />
    public class FrequencyBehavior : SubcircuitBehavior<IFrequencyBehavior>, IFrequencyUpdateBehavior
    {
        private FrequencyParameters _fp;
        private ISubcircuitComplexSimulationState[] _states;

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
            _states = null;

            if (Simulations.Length > 1)
            {
                // Figure out if we need to compute something in parallel
                if (context.Behaviors.Parameters.TryGetValue(out _fp))
                {
                    _states = new ISubcircuitComplexSimulationState[Simulations.Length];
                    for (var i = 0; i < Simulations.Length; i++)
                    {
                        var state = Simulations[i].States.GetValue<IComplexSimulationState>();
                        _states[i] = (ISubcircuitComplexSimulationState)state;
                        state.Setup(Simulations[i]);
                    }
                }

                // Get all shared variables between different tasks
                var common = GetCommonVariables();
                var c = (ComponentBindingContext)context;
                for (var i = 0; i < c.Nodes.Length; i++)
                    common.Add(c.Nodes[i]);
                foreach (var state in _states)
                    state.ShareVariables(common);
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
                foreach (var state in _states)
                    state.Unsetup();
                _states = null;
            }
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
            if (_fp != null && _fp.ParallelInitialize && Behaviors.Length > 1)
            {
                // Execute multiple tasks
                var tasks = new Task[Behaviors.Length];
                for (var t = 0; t < tasks.Length; t++)
                {
                    var task = t;
                    tasks[t] = Task.Run(() =>
                    {
                        foreach (var behavior in Behaviors[task])
                            behavior.InitializeParameters();
                    });
                }
                Task.WaitAll(tasks);
            }
            else
            {
                // Use single thread
                foreach (var bs in Behaviors)
                {
                    foreach (var behavior in bs)
                        behavior.InitializeParameters();
                }
            }
        }

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            if (_fp != null && (_fp.ParallelLoad || _fp.ParallelSolve) && Behaviors.Length > 1)
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
                            foreach (var behavior in Behaviors[task])
                                behavior.Load();
                        }
                        while (!state.ApplyAsynchronously());
                    });
                }
                Task.WaitAll(tasks);
                foreach (var state in _states)
                    state.ApplySynchronously();
            }
            else if (_states != null)
            {
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    do
                    {
                        _states[t].Reset();
                        foreach (var behavior in Behaviors[t])
                            behavior.Load();
                    }
                    while (!_states[t].ApplyAsynchronously());
                }
                foreach (var state in _states)
                    state.ApplySynchronously();
            }
            else
            {
                // Use single thread
                foreach (var bs in Behaviors)
                {
                    foreach (var behavior in bs)
                        behavior.Load();
                }
            }
        }

        /// <summary>
        /// Updates the behavior with the new solution.
        /// </summary>
        void IFrequencyUpdateBehavior.Update()
        {
            if (_states != null)
            {
                foreach (var state in _states)
                    state.Update();
            }
        }
    }
}
