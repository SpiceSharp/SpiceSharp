using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Mosfets.Level1
{
    /// <summary>
    /// Temperature behavior for a <see cref="Mosfet1Model"/>
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITemperatureBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="ModelParameters"/>
    public class ModelTemperature : Behavior, 
        ITemperatureBehavior,
        IParameterized<ModelParameters>
    {
        private readonly ITemperatureSimulationState _temperature;

        /// <inheritdoc/>
        public ModelParameters Parameters { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public ModelProperties Properties { get; } = new ModelProperties();

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelTemperature"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public ModelTemperature(string name, ModelBindingContext context)
            : base(name) 
        {
            context.ThrowIfNull(nameof(context));
            _temperature = context.GetState<ITemperatureSimulationState>();
            Parameters = context.GetParameterSet<ModelParameters>();
        }

        /// <inheritdoc/>
        void ITemperatureBehavior.Temperature()
        {
            // Perform model defaulting
            if (!Parameters.NominalTemperature.Given)
                Parameters.NominalTemperature = new GivenParameter<double>(_temperature.NominalTemperature, false);

            // Update common properties
            Properties.Update(Parameters);

            // Some temperature-dependent parameter changes
            if (Parameters.OxideThickness.Given)
            {
                if (Parameters.SubstrateDoping.Given)
                {
                    if (!Parameters.Phi.Given)
                    {
                        var phi = new GivenParameter<double>(2 * Properties.Vtnom * Math.Log(Parameters.SubstrateDoping * 1e6 / 1.45e16), false);
                        Parameters.Phi = new GivenParameter<double>(Math.Max(.1, phi), false);
                    }

                    var fermis = Parameters.MosfetType * .5 * Parameters.Phi;
                    var wkfng = 3.2;
                    if (!Parameters.GateType.Given)
                        Parameters.GateType = new GivenParameter<double>(1, false);
                    if (!Parameters.GateType.Value.Equals(0))
                    {
                        var fermig = Parameters.MosfetType * Parameters.GateType * .5 * Properties.EgFet1;
                        wkfng = 3.25 + .5 * Properties.EgFet1 - fermig;
                    }

                    var wkfngs = wkfng - (3.25 + .5 * Properties.EgFet1 + fermis);
                    if (!Parameters.Gamma.Given)
                    {
                        Parameters.Gamma = new GivenParameter<double>(
                            Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Constants.Charge * Parameters.SubstrateDoping * 1e6) /
                            Parameters.OxideCapFactor, false);
                    }

                    if (!Parameters.Vt0.Given)
                    {
                        if (!Parameters.SurfaceStateDensity.Given)
                            Parameters.SurfaceStateDensity = new GivenParameter<double>(0, false);
                        var vfb = wkfngs - Parameters.SurfaceStateDensity * 1e4 * Constants.Charge / Parameters.OxideCapFactor;
                        Parameters.Vt0 = new GivenParameter<double>(vfb + Parameters.MosfetType * (Parameters.Gamma * Math.Sqrt(Parameters.Phi) + Parameters.Phi), false);
                    }
                }
            }
        }
    }
}
