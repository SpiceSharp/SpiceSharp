using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.ParameterSets;
using System;

namespace SpiceSharp.Components.MutualInductances
{
    /// <summary>
    /// Temperature-dependent calculations for a <see cref="MutualInductance"/>.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITemperatureBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="MutualInductances.Parameters"/>
    [BehaviorFor(typeof(MutualInductance)), AddBehaviorIfNo(typeof(ITemperatureBehavior))]
    public class Temperature : Behavior,
        ITemperatureBehavior,
        IParameterized<Parameters>
    {
        private readonly Inductors.Temperature _temp1, _temp2;

        /// <summary>
        /// Gets the coupling factor.
        /// </summary>
        protected double Factor { get; private set; }

        /// <inheritdoc/>
        public Parameters Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Temperature"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Temperature(BindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));

            Parameters = context.GetParameterSet<Parameters>();
            _temp1 = (Inductors.Temperature)context.Inductor1Behaviors.GetValue<ITemperatureBehavior>();
            _temp2 = (Inductors.Temperature)context.Inductor2Behaviors.GetValue<ITemperatureBehavior>();
        }

        /// <inheritdoc/>
        void ITemperatureBehavior.Temperature()
        {
            // Calculate coupling factor
            Factor = Parameters.Coupling * Math.Sqrt(_temp1.Inductance * _temp2.Inductance);
        }
    }
}