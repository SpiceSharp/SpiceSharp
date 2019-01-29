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
    /// <param name="name">The name of the entity creating it.</param>
    /// <returns></returns>
    public delegate IBehavior BehaviorFactoryMethod(string name);
}
