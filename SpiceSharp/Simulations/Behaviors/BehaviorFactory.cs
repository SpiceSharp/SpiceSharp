using System;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Factory for behaviors
    /// </summary>
    public class BehaviorFactory : TypeDictionary<BehaviorFactoryDelegate>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BehaviorFactory()
            : base(typeof(Behavior))
        {
        }
    }

    /// <summary>
    /// Delegate
    /// </summary>
    public delegate Behavior BehaviorFactoryDelegate();
}
