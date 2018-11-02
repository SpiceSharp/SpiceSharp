using System;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Factory for behaviors
    /// </summary>
    /// <seealso cref="TypeDictionary{BehaviorFactoryMethod}" />
    public class BehaviorFactoryDictionary : TypeDictionary<BehaviorFactoryMethod>
    {
        /// <summary>
        /// Adds the behavior type and automatically create a factory for it.
        /// </summary>
        /// <param name="type">The behavior type.</param>
        /// <param name="name">The name.</param>
        public void Add(Type type, string name)
        {
            IBehavior Factory() => (IBehavior) Activator.CreateInstance(type, name);
            base.Add(type, Factory);
        }
    }

    /// <summary>
    /// Delegate
    /// </summary>
    /// <returns>The behavior created by the factory.</returns>
    public delegate IBehavior BehaviorFactoryMethod();
}
