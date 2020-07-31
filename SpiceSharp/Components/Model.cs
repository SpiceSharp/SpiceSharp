using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a (Spice) model.
    /// </summary>
    public abstract class Model : Entity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Model"/> class.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        protected Model(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            simulation.EntityBehaviors.Add(behaviors);
        }
    }

    /// <summary>
    /// This class represents a (Spice) model that can be used with dependency injection.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <seealso cref="Entity" />
    public abstract class Model<TContext> : Entity<TContext>
        where TContext : IBindingContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Model{TContext}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        protected Model(string name)
            : base(name)
        {
        }
    }
}
