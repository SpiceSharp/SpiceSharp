using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.VoltageSources
{
    /// <summary>
    /// Accept behavior for a <see cref="VoltageSource"/>
    /// </summary>
    /// <seealso cref="BiasingBehavior"/>
    /// <seealso cref="IAcceptBehavior"/>
    public class Accept : BiasingBehavior,
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

        /// <inheritdoc/>
        void IAcceptBehavior.Probe()
        {
            Waveform?.Probe();
        }

        /// <inheritdoc/>
        void IAcceptBehavior.Accept()
        {
            Waveform?.Accept();
        }
    }
}