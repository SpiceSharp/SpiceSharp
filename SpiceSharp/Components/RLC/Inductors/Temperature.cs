using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.ParameterSets;
using System;

namespace SpiceSharp.Components.Inductors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Inductor"/>.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITemperatureBehavior"/>
    [BehaviorFor(typeof(Inductor)), AddBehaviorIfNo(typeof(ITemperatureBehavior))]
    [GeneratedParameters]
    public partial class Temperature : Behavior,
        ITemperatureBehavior,
        IParameterized<Parameters>
    {
        /// <summary>
        /// Gets the inductance of the inductor.
        /// </summary>
        /// <value>
        /// The inductance.
        /// </value>
        [ParameterName("l"), ParameterName("inductance"), ParameterInfo("The inductance")]
        public double Inductance { get; private set; }

        /// <inheritdoc/>
        public Parameters Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Temperature"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Temperature(IComponentBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            Parameters = context.GetParameterSet<Parameters>();
        }

        /// <inheritdoc/>
        void ITemperatureBehavior.Temperature()
        {
            Inductance = Parameters.Inductance * Parameters.SeriesMultiplier / Parameters.ParallelMultiplier;
        }
    }
}