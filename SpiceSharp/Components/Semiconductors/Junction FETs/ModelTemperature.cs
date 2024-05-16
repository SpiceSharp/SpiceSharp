using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.JFETs
{
    /// <summary>
    /// Temperature behavior for a <see cref="JFETModel" />.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITemperatureBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="ModelParameters"/>
    [BehaviorFor(typeof(JFETModel)), AddBehaviorIfNo(typeof(ITemperatureBehavior))]
    public class ModelTemperature : Behavior,
        ITemperatureBehavior,
        IParameterized<ModelParameters>
    {
        private readonly ITemperatureSimulationState _temperature;

        /// <inheritdoc/>
        public ModelParameters Parameters { get; }

        /// <summary>
        /// Gets the implementation-specific factor 2.
        /// </summary>
        /// <value>
        /// The f2.
        /// </value>
        public double F2 { get; private set; }

        /// <summary>
        /// Gets the implementation-specific factor 3.
        /// </summary>
        /// <value>
        /// The f3.
        /// </value>
        public double F3 { get; private set; }

        /// <summary>
        /// Gets the bulk factor.
        /// </summary>
        /// <value>
        /// The bulk factor.
        /// </value>
        public double BFactor { get; private set; }

        /// <summary>
        /// Gets the implementation-specific factor Pbo.
        /// </summary>
        /// <value>
        /// The pbo.
        /// </value>
        public double Pbo { get; private set; }

        /// <summary>
        /// Gets ???
        /// </summary>
        /// <value>
        /// The XFC.
        /// </value>
        public double Xfc { get; private set; }

        /// <summary>
        /// Gets the junction capacitance factor.
        /// </summary>
        /// <value>
        /// The cjfact.
        /// </value>
        public double Cjfact { get; private set; }

        /// <summary>
        /// Gets the biasing simulation state.
        /// </summary>
        /// <value>
        /// The biasing simulation state.
        /// </value>
        protected IBiasingSimulationState BiasingState { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelTemperature"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public ModelTemperature(IBindingContext context) : base(context)
        {
            context.ThrowIfNull(nameof(context));
            _temperature = context.GetState<ITemperatureSimulationState>();
            Parameters = context.GetParameterSet<ModelParameters>();
            BiasingState = context.GetState<IBiasingSimulationState>();
        }

        /// <inheritdoc/>
        void ITemperatureBehavior.Temperature()
        {
            if (Parameters.NominalTemperature.Given)
                Parameters.NominalTemperature = new GivenParameter<double>(_temperature.NominalTemperature, false);

            double vtnom = Constants.KOverQ * Parameters.NominalTemperature;
            double fact1 = Parameters.NominalTemperature / Constants.ReferenceTemperature;
            double kt1 = Constants.Boltzmann * Parameters.NominalTemperature;
            double egfet1 = 1.16 - (7.02e-4 * Parameters.NominalTemperature * Parameters.NominalTemperature) /
                         (Parameters.NominalTemperature + 1108);
            double arg1 = -egfet1 / (kt1 + kt1) + 1.1150877 / (Constants.Boltzmann * 2 * Constants.ReferenceTemperature);
            double pbfact1 = -2 * vtnom * (1.5 * Math.Log(fact1) + Constants.Charge * arg1);
            Pbo = (Parameters.GatePotential - pbfact1) / fact1;
            double gmaold = (Parameters.GatePotential - Pbo) / Pbo;
            Cjfact = 1 / (1 + .5 * (4e-4 * (Parameters.NominalTemperature - Constants.ReferenceTemperature) - gmaold));

            Xfc = Math.Log(1 - Parameters.DepletionCapCoefficient);
            F2 = Math.Exp((1 + 0.5) * Xfc);
            F3 = 1 - Parameters.DepletionCapCoefficient * (1 + 0.5);
            /* Modification for Sydney University JFET model */
            BFactor = (1 - Parameters.B) / (Parameters.GatePotential - Parameters.Threshold);
        }
    }
}
