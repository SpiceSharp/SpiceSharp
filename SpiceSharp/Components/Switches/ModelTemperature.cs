using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using System;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// Temperature behavior for switches.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IParameterized{T}" />
    /// <seealso cref="ITemperatureBehavior" />
    /// <seealso cref="ModelParameters"/>
    [BehaviorFor(typeof(CurrentSwitchModel), typeof(ITemperatureBehavior))]
    [BehaviorFor(typeof(VoltageSwitchModel), typeof(ITemperatureBehavior))]
    public class ModelTemperature : Behavior,
        IParameterized<ModelParameters>,
        ITemperatureBehavior
    {
        /// <inheritdoc/>
        public ModelParameters Parameters { get; }

        /// <summary>
        /// Gets the on conductance.
        /// </summary>
        /// <value>
        /// The on conductance.
        /// </value>
        public double OnConductance { get; private set; }

        /// <summary>
        /// Gets the off conductance.
        /// </summary>
        /// <value>
        /// The off conductance.
        /// </value>
        public double OffConductance { get; private set; }

        /// <summary>
        /// Gets the threshold parameter.
        /// </summary>
        /// <value>
        /// The threshold value.
        /// </value>
        public double Threshold { get; private set; }

        /// <summary>
        /// Gets the hysteresis parameter.
        /// </summary>
        /// <value>
        /// The hysteresis value.
        /// </value>
        public double Hysteresis { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelTemperature"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public ModelTemperature(IBindingContext context)
            : base(context)
        {
            Parameters = context.GetParameterSet<ModelParameters>();
        }

        /// <inheritdoc/>
        public void Temperature()
        {
            Threshold = Parameters.Threshold;
            Hysteresis = Math.Abs(Parameters.Hysteresis);
            OnConductance = 1.0 / Parameters.OnResistance;
            OffConductance = 1.0 / Parameters.OffResistance;
        }
    }
}
