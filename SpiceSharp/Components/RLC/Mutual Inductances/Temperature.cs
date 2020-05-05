using System;
using SpiceSharp.Behaviors;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.MutualInductances
{
    /// <summary>
    /// Temperature-dependent calculations for a <see cref="MutualInductance"/>.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITemperatureBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="MutualInductances.Parameters"/>
    public class Temperature : Behavior,
        ITemperatureBehavior,
        IParameterized<Parameters>
    {
        private readonly Inductors.Temperature _temp1, _temp2;

        /// <summary>
        /// Gets the coupling factor.
        /// </summary>
        protected double Factor { get; private set; }

        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        public Parameters Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Temperature"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public Temperature(string name, BindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));

            Parameters = context.GetParameterSet<Parameters>();
            _temp1 = (Inductors.Temperature)context.Inductor1Behaviors.GetValue<ITemperatureBehavior>();
            _temp2 = (Inductors.Temperature)context.Inductor2Behaviors.GetValue<ITemperatureBehavior>();
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            // Calculate coupling factor
            Factor = Parameters.Coupling * Math.Sqrt(_temp1.Inductance * _temp2.Inductance);
        }
    }
}