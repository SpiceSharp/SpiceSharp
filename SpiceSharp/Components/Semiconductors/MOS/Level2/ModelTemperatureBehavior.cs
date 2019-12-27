using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
{
    /// <summary>
    /// Temperature behavior for a <see cref="Model"/>
    /// </summary>
    public class ModelTemperatureBehavior : Behavior, ITemperatureBehavior,
        IParameterized<ModelBaseParameters>,
        IParameterized<ModelNoiseParameters>
    {
        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        /// <value>
        /// The model parameters.
        /// </value>
        public ModelBaseParameters Parameters { get; }

        /// <summary>
        /// Gets the noise parameters.
        /// </summary>
        /// <value>
        /// The noise parameters.
        /// </value>
        public ModelNoiseParameters NoiseParameters { get; }

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        ModelNoiseParameters IParameterized<ModelNoiseParameters>.Parameters => NoiseParameters;

        /// <summary>
        /// The permittivity of silicon
        /// </summary>
        protected const double EpsilonSilicon = 11.7 * 8.854214871e-12;

        /// <summary>
        /// Gets the implementation-specific factor 1.
        /// </summary>
        public double Factor1 { get; private set; }

        /// <summary>
        /// Gets the nominal thermal voltage.
        /// </summary>
        public double VtNominal { get; private set; }

        /// <summary>
        /// Gets the band-gap.
        /// </summary>
        public double EgFet1 { get; private set; }

        /// <summary>
        /// Gets the implementation-specific factor PbFactor1
        /// </summary>
        public double PbFactor1 { get; private set; }

        /// <summary>
        /// Gets the implementation-specific Xd.
        /// </summary>
        public double Xd { get; private set; }

        /// <summary>
        /// Gets the biasing simulation state.
        /// </summary>
        /// <value>
        /// The biasing simulation state.
        /// </value>
        protected IBiasingSimulationState BiasingState { get; private set; }
        private readonly ITemperatureSimulationState _temperature;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelTemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public ModelTemperatureBehavior(string name, ModelBindingContext context) : base(name) 
        {
            context.ThrowIfNull(nameof(context));
            NoiseParameters = context.GetParameterSet<ModelNoiseParameters>();
            _temperature = context.GetState<ITemperatureSimulationState>();
            Parameters = context.GetParameterSet<ModelBaseParameters>();
            BiasingState = context.GetState<IBiasingSimulationState>();
        }

        /// <summary>
        /// Do temperature-dependent calculations.
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            // Perform model defaulting
            if (!Parameters.NominalTemperature.Given)
                Parameters.NominalTemperature.RawValue = _temperature.NominalTemperature;
            Factor1 = Parameters.NominalTemperature / Constants.ReferenceTemperature;
            VtNominal = Parameters.NominalTemperature * Constants.KOverQ;
            var kt1 = Constants.Boltzmann * Parameters.NominalTemperature;
            EgFet1 = 1.16 - 7.02e-4 * Parameters.NominalTemperature * Parameters.NominalTemperature / (Parameters.NominalTemperature + 1108);
            var arg1 = -EgFet1 / (kt1 + kt1) + 1.1150877 / (Constants.Boltzmann * (Constants.ReferenceTemperature + Constants.ReferenceTemperature));
            PbFactor1 = -2 * VtNominal * (1.5 * Math.Log(Factor1) + Constants.Charge * arg1);

            if (Parameters.SubstrateDoping.Given)
            {
                if (Parameters.SubstrateDoping * 1e6 > 1.45e16)
                {
                    if (!Parameters.Phi.Given)
                    {
                        Parameters.Phi.RawValue = 2 * VtNominal * Math.Log(Parameters.SubstrateDoping * 1e6 / 1.45e16);
                        Parameters.Phi.RawValue = Math.Max(.1, Parameters.Phi);
                    }
                    var fermis = Parameters.MosfetType * .5 * Parameters.Phi;
                    var wkfng = 3.2;
                    if (!Parameters.GateType.Given)
                        Parameters.GateType.RawValue = 1;
                    if (!Parameters.GateType.RawValue.Equals(0))
                    {
                        var fermig = Parameters.MosfetType * Parameters.GateType * .5 * EgFet1;
                        wkfng = 3.25 + .5 * EgFet1 - fermig;
                    }
                    var wkfngs = wkfng - (3.25 + .5 * EgFet1 + fermis);
                    if (!Parameters.Gamma.Given)
                    {
                        Parameters.Gamma.RawValue = Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Constants.Charge * Parameters.SubstrateDoping * 1e6) / Parameters.OxideCapFactor;
                    }
                    if (!Parameters.Vt0.Given)
                    {
                        if (!Parameters.SurfaceStateDensity.Given)
                            Parameters.SurfaceStateDensity.RawValue = 0;
                        var vfb = wkfngs - Parameters.SurfaceStateDensity * 1e4 * Constants.Charge / Parameters.OxideCapFactor;
                        Parameters.Vt0.RawValue = vfb + Parameters.MosfetType * (Parameters.Gamma * Math.Sqrt(Parameters.Phi) + Parameters.Phi);
                    }

                    Xd = Math.Sqrt((EpsilonSilicon + EpsilonSilicon) / (Constants.Charge * Parameters.SubstrateDoping * 1e6));
                }
                else
                {
                    Parameters.SubstrateDoping.RawValue = 0;
                    throw new BadParameterException("Nsub", Properties.Resources.Mosfets_NsubTooSmall);
                }
            }
            if (!Parameters.BulkCapFactor.Given)
            {
                Parameters.BulkCapFactor.RawValue = Math.Sqrt(EpsilonSilicon * Constants.Charge * Parameters.SubstrateDoping * 1e6 /* cm**3/m**3 */  / (2 *
                    Parameters.BulkJunctionPotential));
            }
        }
    }
}
