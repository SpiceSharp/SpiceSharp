using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Mosfets.Level2
{
    /// <summary>
    /// Temperature behavior for a <see cref="Mosfet2Model"/>
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITemperatureBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="ModelParameters"/>
    [BehaviorFor(typeof(Mosfet2Model)), AddBehaviorIfNo(typeof(ITemperatureBehavior))]
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
        /// The permittivity of silicon.
        /// </summary>
        protected const double EpsilonSilicon = 11.7 * 8.854214871e-12;

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
        public ModelTemperature(IBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            _temperature = context.GetState<ITemperatureSimulationState>();
            Parameters = context.GetParameterSet<ModelParameters>();
            BiasingState = context.GetState<IBiasingSimulationState>();
        }

        /// <inheritdoc/>
        /// <exception cref="SpiceSharpException">
        /// Thrown if Phi is not positive or if Nsub is smaller than Ni.
        /// </exception>
        void ITemperatureBehavior.Temperature()
        {
            if (!Parameters.NominalTemperature.Given)
                Parameters.NominalTemperature = new GivenParameter<double>(_temperature.NominalTemperature, false);
            Properties.Factor1 = Parameters.NominalTemperature / Constants.ReferenceTemperature;
            Properties.Vtnom = Parameters.NominalTemperature * Constants.KOverQ;
            Properties.Kt1 = Constants.Boltzmann * Parameters.NominalTemperature;
            Properties.EgFet1 = 1.16 - (7.02e-4 * Parameters.NominalTemperature * Parameters.NominalTemperature) /
                    (Parameters.NominalTemperature + 1108);
            double arg1 = -Properties.EgFet1 / (Properties.Kt1 + Properties.Kt1) + 1.1150877 / (Constants.Boltzmann * (Constants.ReferenceTemperature + Constants.ReferenceTemperature));
            Properties.PbFactor1 = -2 * Properties.Vtnom * (1.5 * Math.Log(Properties.Factor1) + Constants.Charge * arg1);

            if (Parameters.Phi <= 0.0)
                throw new SpiceSharpException("{0}: Phi is not positive.".FormatString(Name));
            Properties.OxideCapFactor = 3.9 * 8.854214871e-12 / Parameters.OxideThickness;

            if (!Parameters.Transconductance.Given)
                Parameters.Transconductance = Parameters.SurfaceMobility * 1e-4 /*(m**2/cm**2) */ * Properties.OxideCapFactor;
            if (Parameters.SubstrateDoping.Given)
            {
                if (Parameters.SubstrateDoping * 1e6 /*(cm**3/m**3)*/ > 1.45e16)
                {
                    if (!Parameters.Phi.Given)
                    {
                        Parameters.Phi = 2 * Properties.Vtnom * Math.Log(Parameters.SubstrateDoping * 1e6 /*(cm**3/m**3)*// 1.45e16);
                        Parameters.Phi = new GivenParameter<double>(Math.Max(.1, Parameters.Phi));
                    }
                    double fermis = Parameters.MosfetType * .5 * Parameters.Phi;
                    double wkfng = 3.2;
                    if (Parameters.GateType != 0)
                    {
                        double fermig = Parameters.MosfetType * Parameters.GateType * .5 * Properties.EgFet1;
                        wkfng = 3.25 + .5 * Properties.EgFet1 - fermig;
                    }
                    double wkfngs = wkfng - (3.25 + .5 * Properties.EgFet1 + fermis);
                    if (!Parameters.Gamma.Given)
                        Parameters.Gamma = Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Constants.Charge * Parameters.SubstrateDoping * 1e6 /*(cm**3/m**3)*/) / Properties.OxideCapFactor;
                    if (!Parameters.Vt0.Given)
                    {
                        double vfb = wkfngs - Parameters.SurfaceStateDensity * 1e4 /*(cm**2/m**2)*/ * Constants.Charge / Properties.OxideCapFactor;
                        Parameters.Vt0 = vfb + Parameters.MosfetType * (Parameters.Gamma * Math.Sqrt(Parameters.Phi) + Parameters.Phi);
                    }
                    Properties.Xd = Math.Sqrt((EpsilonSilicon + EpsilonSilicon) / (Constants.Charge * Parameters.SubstrateDoping * 1e6 /*(cm**3/m**3)*/));
                }
                else
                {
                    Parameters.SubstrateDoping = 0;
                    throw new SpiceSharpException("{0}: Nsub < Ni.".FormatString(Name));
                }
            }
            if (!Parameters.BulkCapFactor.Given)
            {
                Parameters.BulkCapFactor = new GivenParameter<double>(Math.Sqrt(EpsilonSilicon * Constants.Charge *
                    Parameters.SubstrateDoping * 1e6 /*cm**3/m**3*/ / (2 * Parameters.BulkJunctionPotential)), false);
            }
        }
    }
}
