using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.CurrentSources
{
    /// <summary>
    /// Accept behavior for a <see cref="CurrentSource"/>.
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
        public Accept(string name, IComponentBindingContext context)
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