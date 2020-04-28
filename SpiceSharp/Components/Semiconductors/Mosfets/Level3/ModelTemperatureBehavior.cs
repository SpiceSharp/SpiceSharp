using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Temperature behavior for a <see cref="Mosfet3Model" />.
    /// </summary>
    public class ModelTemperatureBehavior : Behavior, ITemperatureBehavior,
        IParameterized<ModelBaseParameters>,
        IParameterized<ModelNoiseParameters>
    {
        private readonly ITemperatureSimulationState _temperature;

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
        ModelNoiseParameters IParameterized<ModelNoiseParameters>.Parameters => NoiseParameters;

        /// <summary>
        /// The permittivity of silicon
        /// </summary>
        protected const double EpsilonSilicon = 11.7 * 8.854214871e-12;

        /// <summary>
        /// Gets the width of the depletion layer.
        /// </summary>
        [ParameterName("xd"), ParameterInfo("Depletion layer width")]
        public double CoefficientDepletionLayerWidth { get; private set; }

        /// <summary>
        /// Gets alpha.
        /// </summary>
        [ParameterName("alpha"), ParameterInfo("Alpha")]
        public double Alpha { get; private set; }

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
        }

        /// <summary>
        /// Do temperature-dependent calculations.
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            // Perform model defaulting
            if (!Parameters.NominalTemperature.Given)
                Parameters.NominalTemperature = new GivenParameter<double>(_temperature.NominalTemperature, false);
            Factor1 = Parameters.NominalTemperature / Constants.ReferenceTemperature;
            VtNominal = Parameters.NominalTemperature * Constants.KOverQ;
            var kt1 = Constants.Boltzmann * Parameters.NominalTemperature;
            EgFet1 = 1.16 - 7.02e-4 * Parameters.NominalTemperature * Parameters.NominalTemperature / (Parameters.NominalTemperature + 1108);
            var arg1 = -EgFet1 / (kt1 + kt1) + 1.1150877 / (Constants.Boltzmann * (Constants.ReferenceTemperature + Constants.ReferenceTemperature));
            PbFactor1 = -2 * VtNominal * (1.5 * Math.Log(Factor1) + Constants.Charge * arg1);

            if (Parameters.SubstrateDoping.Given)
            {
                if (!Parameters.Phi.Given)
                {
                    Parameters.Phi = new GivenParameter<double>(2 * VtNominal * Math.Log(Parameters.SubstrateDoping * 1e6 /* (cm^3/m^3) */  / 1.45e16), false);
                    Parameters.Phi = new GivenParameter<double>(Math.Max(0.1, Parameters.Phi), false);
                }
                var fermis = Parameters.MosfetType * 0.5 * Parameters.Phi;
                var wkfng = 3.2;
                if (!Parameters.GateType.Given)
                    Parameters.GateType = new GivenParameter<double>(1, false);
                if (!Parameters.GateType.Value.Equals(0.0))
                {
                    var fermig = Parameters.MosfetType * Parameters.GateType * 0.5 * EgFet1;
                    wkfng = 3.25 + 0.5 * EgFet1 - fermig;
                }
                var wkfngs = wkfng - (3.25 + 0.5 * EgFet1 + fermis);
                if (!Parameters.Gamma.Given)
                {
                    Parameters.Gamma = new GivenParameter<double>(Math.Sqrt(2 * EpsilonSilicon * Constants.Charge * Parameters.SubstrateDoping * 1e6 /* (cm**3 / m**3) */) /
                                          Parameters.OxideCapFactor, false);
                }
                if (!Parameters.Vt0.Given)
                {
                    if (!Parameters.SurfaceStateDensity.Given)
                        Parameters.SurfaceStateDensity = new GivenParameter<double>(0, false);
                    var vfb = wkfngs - Parameters.SurfaceStateDensity * 1e4 * Constants.Charge / Parameters.OxideCapFactor;
                    Parameters.Vt0 = new GivenParameter<double>(vfb + Parameters.MosfetType * (Parameters.Gamma * Math.Sqrt(Parameters.Phi) + Parameters.Phi), false);
                }

                Alpha = (EpsilonSilicon + EpsilonSilicon) / (Constants.Charge * Parameters.SubstrateDoping * 1e6 /* (cm**3 / m**3) */);
                CoefficientDepletionLayerWidth = Math.Sqrt(Alpha);
            }
        }
    }
}
