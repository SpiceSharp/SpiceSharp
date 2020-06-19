using SpiceSharp.Behaviors;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Inductors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Inductor"/>.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITemperatureBehavior"/>
    public class Temperature : Behavior,
        ITemperatureBehavior
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
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public Temperature(string name, IComponentBindingContext context)
            : base(name)
        {
            context.ThrowIfNull(nameof(context));
            Parameters = context.GetParameterSet<Parameters>();
        }

        void ITemperatureBehavior.Temperature()
        {
            Inductance = Parameters.Inductance * Parameters.SeriesMultiplier / Parameters.ParallelMultiplier;
        }
    }
}