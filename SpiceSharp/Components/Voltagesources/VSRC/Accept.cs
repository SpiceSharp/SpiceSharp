using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.VoltageSources
{
    /// <summary>
    /// Accept behavior for a <see cref="VoltageSource"/>
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IAcceptBehavior"/>
    [BehaviorFor(typeof(VoltageSource)), AddBehaviorIfNo(typeof(IAcceptBehavior))]
    [GeneratedParameters]
    public partial class Accept : Biasing,
        IAcceptBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Accept"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Accept(IComponentBindingContext context)
            : base(context)
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