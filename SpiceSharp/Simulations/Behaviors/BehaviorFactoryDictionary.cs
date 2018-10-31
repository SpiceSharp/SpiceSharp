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
    /// Delegate
    /// </summary>
    /// <returns>The behavior created by the factory.</returns>
    public delegate IBehavior BehaviorFactoryMethod();
}
