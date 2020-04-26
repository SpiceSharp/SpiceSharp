using SpiceSharp.Behaviors;

namespace SpiceSharp.Components
{
    public partial class CurrentSource
    {
        /// <summary>
        /// Accept behavior for a <see cref="CurrentSource"/>.
        /// </summary>
        /// <seealso cref="BiasingBehavior"/>
        /// <seealso cref="IAcceptBehavior"/>
        protected class AcceptBehavior : BiasingBehavior, IAcceptBehavior
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AcceptBehavior"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="context">The context.</param>
            public AcceptBehavior(string name, ComponentBindingContext context)
                : base(name, context)
            {
            }

            void IAcceptBehavior.Probe()
            {
                Waveform?.Probe();
            }

            void IAcceptBehavior.Accept()
            {
                Waveform?.Accept();
            }
        }
    }
}
