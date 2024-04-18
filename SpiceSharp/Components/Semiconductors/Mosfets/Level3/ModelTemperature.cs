using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Mosfets.Level3
{
    /// <summary>
    /// Temperature behavior for a <see cref="Mosfet3Model"/>
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITemperatureBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="ModelParameters"/>
    [BehaviorFor(typeof(Mosfet3Model)), AddBehaviorIfNo(typeof(ITemperatureBehavior))]
    public class ModelTemperature : Behavior,
        ITemperatureBehavior,
        IParameterized<ModelParameters>
    {
        private readonly ITemperatureSimulationState _temperature;

        /// <inheritdoc/>
        public ModelParameters Parameters { get; }

        /// <summary>
        /// Gets the model properties.
        /// </summary>
        /// <value>
        /// The model properties.
        /// </value>
        public ModelProperties Properties { get; } = new ModelProperties();

        /// <summary>
        /// The permittivity of silicon
        /// </summary>
        protected const double EpsilonSilicon = 11.7 * 8.854214871e-12;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelTemperature"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public ModelTemperature(IBindingContext context)
            : base(context)
        {
            _temperature = context.GetState<ITemperatureSimulationState>();
            Parameters = context.GetParameterSet<ModelParameters>();
        }

        /// <inheritdoc/>
        void ITemperatureBehavior.Temperature()
        {
            if (!Parameters.NominalTemperature.Given)
                Parameters.NominalTemperature = _temperature.Temperature;
            Properties.Factor1 = Parameters.NominalTemperature / Constants.ReferenceTemperature;
            Properties.Vtnom = Parameters.NominalTemperature * Constants.KOverQ;
            Properties.Kt1 = Constants.Boltzmann * Parameters.NominalTemperature;
            Properties.EgFet1 = 1.16 - (7.02e-4 * Parameters.NominalTemperature * Parameters.NominalTemperature) /
                    (Parameters.NominalTemperature + 1108);
            double arg1 = -Properties.EgFet1 / (Properties.Kt1 + Properties.Kt1) + 1.1150877 / (Constants.Boltzmann * (Constants.ReferenceTemperature + Constants.ReferenceTemperature));
            Properties.PbFactor1 = -2 * Properties.Vtnom * (1.5 * Math.Log(Properties.Factor1) + Constants.Charge * arg1);

            double nifact = 1.0;
            if (Parameters.Version == ModelParameters.Versions.NgSpice)
            {
                nifact = (Parameters.NominalTemperature / 300) * Math.Sqrt(Parameters.NominalTemperature / 300);
                nifact *= Math.Exp(0.5 * Properties.EgFet1 * ((1 / (double)300) - (1 / Parameters.NominalTemperature)) / Constants.KOverQ);
            }
            double ni_temp = 1.45e16 * nifact;

            if (Parameters.Phi <= 0.0)
                throw new SpiceSharpException("{0}: Phi is not positive.".FormatString(Name));
            Properties.OxideCapFactor = 3.9 * 8.854214871e-12 / Parameters.OxideThickness;
            if (!Parameters.Transconductance.Given)
                Parameters.Transconductance = Parameters.SurfaceMobility * Properties.OxideCapFactor * 1e-4;
            if (Parameters.SubstrateDoping.Given)
            {
                if (Parameters.SubstrateDoping * 1e6 /*(cm**3/m**3)*/ > ni_temp)
                {
                    if (!Parameters.Phi.Given)
                    {
                        Parameters.Phi = 2 * Properties.Vtnom *
                                Math.Log(Parameters.SubstrateDoping *
                               1e6/*(cm**3/m**3)*// ni_temp);
                        Parameters.Phi = Math.Max(.1, Parameters.Phi);
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
                        Parameters.Gamma = Math.Sqrt(2 * EpsilonSilicon * Constants.Charge * Parameters.SubstrateDoping * 1e6 /*(cm**3/m**3)*/ ) / Properties.OxideCapFactor;
                    if (!Parameters.Vt0.Given)
                    {
                        double vfb = wkfngs - Parameters.SurfaceStateDensity * 1e4 * Constants.Charge / Properties.OxideCapFactor;
                        Parameters.Vt0 = vfb + Parameters.MosfetType * (Parameters.Gamma * Math.Sqrt(Parameters.Phi) + Parameters.Phi);
                    }

                    Properties.Alpha = (EpsilonSilicon + EpsilonSilicon) /
                        (Constants.Charge * Parameters.SubstrateDoping * 1e6 /*(cm**3/m**3)*/ );
                    Properties.CoeffDepLayWidth = Math.Sqrt(Properties.Alpha);
                }
                else
                {
                    Parameters.SubstrateDoping = 0;
                    throw new SpiceSharpException("{0}: Nsub < Ni.".FormatString(Name));
                }
            }
            // Now model parameter preprocessing
            Properties.NarrowFactor = Parameters.Delta * 0.5 * Math.PI * EpsilonSilicon / Properties.OxideCapFactor;
        }
    }
}
