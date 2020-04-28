using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components
{
    public partial class Inductor
    {
        /// <summary>
        /// Temperature behavior for a <see cref="Inductor"/>.
        /// </summary>
        /// <seealso cref="Behavior"/>
        /// <seealso cref="ITemperatureBehavior"/>
        /// <seealso cref="IInductanceBehavior"/>
        protected partial class TemperatureBehavior : Behavior,
            ITemperatureBehavior, 
            IInductanceBehavior
        {
            /// <summary>
            /// Gets the inductance of the inductor.
            /// </summary>
            /// <value>
            /// The inductance.
            /// </value>
            [ParameterName("l"), ParameterName("inductance"), ParameterInfo("The inductance")]
            public double Inductance { get; private set; }

            /// <summary>
            /// Gets the parameters.
            /// </summary>
            /// <value>
            /// The parameters.
            /// </value>
            public InductorParameters Parameters { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="context">The context.</param>
            public TemperatureBehavior(string name, IComponentBindingContext context)
                : base(name)
            {
                context.ThrowIfNull(nameof(context));
                Parameters = context.GetParameterSet<InductorParameters>();
            }

            /// <summary>
            /// Do temperature-dependent calculations
            /// </summary>
            void ITemperatureBehavior.Temperature()
            {
                Inductance = Parameters.Inductance * Parameters.SeriesMultiplier / Parameters.ParallelMultiplier;
            }
        }
    }
}