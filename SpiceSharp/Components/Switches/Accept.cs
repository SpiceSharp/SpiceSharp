using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// Accepting behavior for switches.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IAcceptBehavior"/>
    [BehaviorFor(typeof(CurrentSwitch))]
    [BehaviorFor(typeof(VoltageSwitch))]
    [AddBehaviorIfNo(typeof(IAcceptBehavior))]
    [GeneratedParameters]
    public partial class Accept : Biasing,
        IAcceptBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Accept" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Accept(ISwitchBindingContext context)
            : base(context)
        {
        }

        /// <inheritdoc/>
        void IAcceptBehavior.Probe()
        {
        }

        /// <inheritdoc/>
        void IAcceptBehavior.Accept()
        {
            UseOldState = true;
            PreviousState = CurrentState;
        }
    }
}
