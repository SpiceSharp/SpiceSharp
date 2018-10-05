namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Factory for behaviors
    /// </summary>
    /// <seealso cref="TypeDictionary{BehaviorFactoryMethod}" />
    public class BehaviorFactoryDictionary : TypeDictionary<BehaviorFactoryMethod>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BehaviorFactoryDictionary()
            : base(typeof(Behavior))
        {
        }
    }

    /// <summary>
    /// Delegate
    /// </summary>
    /// <returns>The behavior created by the factory.</returns>
    public delegate Behavior BehaviorFactoryMethod();
}
