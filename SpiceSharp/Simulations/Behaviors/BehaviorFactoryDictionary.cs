using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Factory for behaviors
    /// </summary>
    /// <seealso cref="TypeDictionary{BehaviorFactoryMethod}" />
    public class BehaviorFactoryDictionary : TypeDictionary<BehaviorFactoryMethod>
    {
    }

    /// <summary>
    /// Create a behavior for an entity
    /// </summary>
    /// <param name="entity">The entity creating the behavior.</param>
    /// <returns></returns>
    public delegate IBehavior BehaviorFactoryMethod(Entity entity);
}
