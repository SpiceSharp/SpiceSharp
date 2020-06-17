using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// Accepting behavior for switches.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IAcceptBehavior"/>
    public class Accept : Biasing, 
        IAcceptBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Accept"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        /// <param name="controller">The controller.</param>
        public Accept(string name, IComponentBindingContext context, Controller controller) 
            : base(name, context, controller)
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
