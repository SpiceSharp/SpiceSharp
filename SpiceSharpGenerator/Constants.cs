namespace SpiceSharpGenerator
{
    /// <summary>
    /// Constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The base behavior interface.
        /// </summary>
        public const string IBehavior = "IBehavior";

        /// <summary>
        /// The namespace for behaviors.
        /// </summary>
        public const string BehaviorNamespace = "SpiceSharp.Behaviors";

        /// <summary>
        /// An attribute that flags simulation behaviors.
        /// </summary>
        public const string SimulationBehavior = "SimulationBehaviorAttribute";

        /// <summary>
        /// An attribute that flags a behavior for an entity.
        /// </summary>
        public const string BehaviorFor = "BehaviorForAttribute";

        /// <summary>
        /// An attribute that indicates which behavior should be checked.
        /// </summary>
        public const string AddBehaviorIfNo = "AddBehaviorIfNoAttribute";

        /// <summary>
        /// An attribute that flags a behavior requiring another behavior.
        /// </summary>
        public const string BehaviorRequires = "BehaviorRequiresAttribute";

        /// <summary>
        /// An attribute that flags a binding context for an entity.
        /// </summary>
        public const string BindingContextFor = "BindingContextForAttribute";

        /// <summary>
        /// The namespace that all attributes are stored in.
        /// </summary>
        public const string AttributeNamespace = "SpiceSharp.Attributes";
    }
}
