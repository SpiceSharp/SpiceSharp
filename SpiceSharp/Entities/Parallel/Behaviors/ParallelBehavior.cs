using SpiceSharp.Behaviors;
using System;
using System.Threading.Tasks;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// A behavior that stores a list of sub-behaviors.
    /// </summary>
    /// <typeparam name="T">The behavior type.</typeparam>
    /// <seealso cref="Behavior" />
    public abstract class ParallelBehavior<T> : Behavior where T : IBehavior
    {
        /// <summary>
        /// Gets the behaviors.
        /// </summary>
        /// <value>
        /// The behaviors.
        /// </value>
        protected BehaviorList<T>[] Behaviors { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelBehavior{T}"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public ParallelBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);
            var pc = (ParallelBindingContext)context;
            Behaviors = new BehaviorList<T>[pc.Simulations.Length];
            for (var i = 0; i < Behaviors.Length; i++)
                Behaviors[i] = pc.Simulations[i].EntityBehaviors.GetBehaviorList<T>();
        }

        /// <summary>
        /// Destroy the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();

            if (Behaviors != null)
            {
                for (var i = 0; i < Behaviors.Length; i++)
                {
                    var blist = Behaviors[i];
                    for (var k = 0; k < blist.Count; k++)
                        blist[k].Unbind();
                }
                Behaviors = null;
            }
        }

        /// <summary>
        /// Run a behavior method in parallel.
        /// </summary>
        /// <param name="method">The method.</param>
        protected void For(Func<T, Action> method)
        {
            if (Behaviors.Length > 1)
            {
                var tasks = new Task[Behaviors.Length];
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var b = Behaviors[t];
                    if (b.Count == 0)
                        continue;
                    tasks[t] = Task.Run(() =>
                    {
                        for (var i = 0; i < b.Count; i++)
                            method(b[i]).Invoke();
                    });
                }
                Task.WaitAll(tasks);
            }
            else
            {
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var b = Behaviors[t];
                    if (b.Count == 0)
                        continue;
                    for (var i = 0; i < b.Count; i++)
                        method(b[i]).Invoke();
                }
            }
        }

        /// <summary>
        /// Run a behavior method in parallel and do boolean AND on all results.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>
        /// The boolean AND result.
        /// </returns>
        protected bool ForAnd(Func<T, Func<bool>> method)
        {
            if (Behaviors.Length > 1)
            {
                var tasks = new Task<bool>[Behaviors.Length];
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var b = Behaviors[t];
                    if (b.Count == 0)
                        continue;
                    tasks[t] = Task.Run(() =>
                    {
                        var result = true;
                        for (var i = 0; i < b.Count; i++)
                            result &= method(b[i]).Invoke();
                        return result;
                    });
                }
                Task.WaitAll(tasks);
                var result = true;
                foreach (var task in tasks)
                    result &= task.Result;
                return result;
            }
            else
            {
                var result = true;
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var b = Behaviors[t];
                    if (b.Count == 0)
                        continue;
                    for (var i = 0; i < b.Count; i++)
                        result &= method(b[i]).Invoke();
                }
                return result;
            }
        }
    }
}
