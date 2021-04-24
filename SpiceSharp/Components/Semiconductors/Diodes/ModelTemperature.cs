using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Diodes
{
    /// <summary>
    /// Temperature behavior for a <see cref="DiodeModel"/>
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITemperatureBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="ModelParameters"/>
    [BehaviorFor(typeof(DiodeModel)), AddBehaviorIfNo(typeof(ITemperatureBehavior))]
    public class ModelTemperature : Behavior,
        ITemperatureBehavior,
        IParameterized<ModelParameters>
    {
        /// <inheritdoc/>
        public ModelParameters Parameters { get; }

        /// <summary>
        /// The ohmic conductance.
        /// </summary>
        /// <value>
        /// The ohmic conductance.
        /// </value>
        [ParameterName("cond"), ParameterInfo("Ohmic conductance")]
        public double Conductance { get; protected set; }

        /// <summary>
        /// Gets the nominal thermal voltage.
        /// </summary>
        /// <value>
        /// The nominal thermal voltage.
        /// </value>
        public double VtNominal { get; protected set; }

        /// <summary>
        /// Gets ???
        /// </summary>
        /// <value>
        /// The XFC.
        /// </value>
        public double Xfc { get; protected set; }

        /// <summary>
        /// Gets the implementation-specific factor 2.
        /// </summary>
        /// <value>
        /// The f2.
        /// </value>
        public double F2 { get; protected set; }

        /// <summary>
        /// Gets the implementation-specific factor 3.
        /// </summary>
        /// <value>
        /// The f3.
        /// </value>
        public double F3 { get; protected set; }

        private readonly ITemperatureSimulationState _temperature;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelTemperature"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public ModelTemperature(IBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            _temperature = context.GetState<ITemperatureSimulationState>();
            Parameters = context.GetParameterSet<ModelParameters>();
        }

        /// <inheritdoc/>
        void ITemperatureBehavior.Temperature()
        {
            if (!Parameters.NominalTemperature.Given)
                Parameters.NominalTemperature = new GivenParameter<double>(_temperature.NominalTemperature, false);
            VtNominal = Constants.KOverQ * Parameters.NominalTemperature;

            if (!Parameters.Resistance.Equals(0.0))
                Conductance = 1 / Parameters.Resistance;
            else
                Conductance = 0;
            Xfc = Math.Log(1 - Parameters.DepletionCapCoefficient);

            F2 = Math.Exp((1 + Parameters.GradingCoefficient) * Xfc);
            F3 = 1 - Parameters.DepletionCapCoefficient * (1 + Parameters.GradingCoefficient);
        }
    }
}
