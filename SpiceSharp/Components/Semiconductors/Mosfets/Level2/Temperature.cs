using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Mosfets.Level2
{
    /// <summary>
    /// Temperature behavior for a <see cref="Mosfet2"/>.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITemperatureBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    [BehaviorFor(typeof(Mosfet2)), AddBehaviorIfNo(typeof(ITemperatureBehavior))]
    public class Temperature : Behavior,
        ITemperatureBehavior,
        IParameterized<Parameters>
    {
        private readonly ITemperatureSimulationState _temperature;

        /// <inheritdoc/>
        public Parameters Parameters { get; }

        /// <summary>
        /// The common temperature-dependent properties.
        /// </summary>
        protected readonly TemperatureProperties Properties = new();

        /// <summary>
        /// The model parameters.
        /// </summary>
        protected readonly ModelParameters ModelParameters;

        /// <summary>
        /// The model temperature behavior.
        /// </summary>
        protected readonly ModelTemperature ModelTemperature;

        /// <include file='../common/docs.xml' path='docs/members/SourceConductance/*'/>
        [ParameterName("sourceconductance"), ParameterInfo("Conductance at the source")]
        public double SourceConductance => Properties.SourceConductance.Equals(0.0) ? double.PositiveInfinity : Properties.SourceConductance;

        /// <include file='../common/docs.xml' path='docs/members/DrainConductance/*'/>
        [ParameterName("drainconductance"), ParameterInfo("Conductance at the drain")]
        public double DrainConductance => Properties.DrainConductance.Equals(0.0) ? double.PositiveInfinity : Properties.DrainConductance;

        /// <include file='../common/docs.xml' path='docs/members/SourceResistance/*'/>
        [ParameterName("rs"), ParameterInfo("Source resistance")]

        public double SourceResistance => Properties.SourceConductance.Equals(0.0) ? 0 : 1.0 / Properties.SourceConductance;

        /// <include file='../common/docs.xml' path='docs/members/DrainResistance/*'/>
        [ParameterName("rd"), ParameterInfo("Drain conductance")]
        public double DrainResistance => Properties.DrainConductance.Equals(0.0) ? 0 : 1.0 / Properties.DrainConductance;

        /// <include file='../common/docs.xml' path='docs/members/CriticalSourceVoltage/*'/>
        [ParameterName("sourcevcrit"), ParameterInfo("Critical source voltage")]
        public double SourceVCritical => Properties.SourceVCritical;

        /// <include file='../common/docs.xml' path='docs/members/CriticalDrainVoltage/*'/>
        [ParameterName("drainvcrit"), ParameterInfo("Critical drain voltage")]
        public double DrainVCritical => Properties.DrainVCritical;

        /// <summary>
        /// Initializes a new instance of the <see cref="Temperature"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Temperature(IComponentBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            _temperature = context.GetState<ITemperatureSimulationState>();
            ModelTemperature = context.ModelBehaviors.GetValue<ModelTemperature>();
            ModelParameters = ModelTemperature.GetParameterSet<ModelParameters>();
            Parameters = context.GetParameterSet<Parameters>();
        }

        /// <inheritdoc/>
        /// <exception cref="SpiceSharpException">
        /// Thrown if the effective channel length is less than 0.
        /// </exception>
        void ITemperatureBehavior.Temperature()
        {
            if (!Parameters.Temperature.Given)
                Parameters.Temperature = new GivenParameter<double>(_temperature.Temperature + Parameters.DeltaTemperature, false);
            // Parameters.ParallelMultiplierode = 1;
            // here->MOS2von = 0;

            Properties.TempVt = Parameters.Temperature * Constants.KOverQ;
            double ratio = Parameters.Temperature / ModelParameters.NominalTemperature;
            double fact2 = Parameters.Temperature / Constants.ReferenceTemperature;
            double kt = Parameters.Temperature * Constants.Boltzmann;
            double egfet = 1.16 - (7.02e-4 * Parameters.Temperature * Parameters.Temperature) /
                    (Parameters.Temperature + 1108);
            double arg = -egfet / (kt + kt) + 1.1150877 / (Constants.Boltzmann * (Constants.ReferenceTemperature + Constants.ReferenceTemperature));
            double pbfact = -2 * Properties.TempVt * (1.5 * Math.Log(fact2) + Constants.Charge * arg);

            if (ModelParameters.DrainResistance.Given)
            {
                if (ModelParameters.DrainResistance != 0)
                    Properties.DrainConductance = Parameters.ParallelMultiplier / ModelParameters.DrainResistance;
                else
                    Properties.DrainConductance = 0;
            }
            else if (ModelParameters.SheetResistance.Given)
            {
                if ((ModelParameters.SheetResistance != 0) && (Parameters.DrainSquares != 0))
                    Properties.DrainConductance = Parameters.ParallelMultiplier / (ModelParameters.SheetResistance * Parameters.DrainSquares);
                else
                    Properties.DrainConductance = 0;
            }
            else
                Properties.DrainConductance = 0;

            if (ModelParameters.SourceResistance.Given)
            {
                if (ModelParameters.SourceResistance != 0)
                    Properties.SourceConductance = Parameters.ParallelMultiplier / ModelParameters.SourceResistance;
                else
                    Properties.SourceConductance = 0;
            }
            else if (ModelParameters.SheetResistance.Given)
            {
                if (ModelParameters.SheetResistance != 0 && Parameters.SourceSquares != 0)
                    Properties.SourceConductance = Parameters.ParallelMultiplier / (ModelParameters.SheetResistance * Parameters.SourceSquares);
                else
                    Properties.SourceConductance = 0;
            }
            else
                Properties.SourceConductance = 0;

            if (Parameters.Length - 2 * ModelParameters.LateralDiffusion <= 0)
                throw new SpiceSharpException("{0}, {1}: Effective channel length is less than zero".FormatString(ModelTemperature.Name, Name));

            double ratio4 = ratio * Math.Sqrt(ratio);
            Properties.TempTransconductance = ModelParameters.Transconductance / ratio4;
            Properties.TempSurfaceMobility = ModelParameters.SurfaceMobility / ratio4;
            double phio = (ModelParameters.Phi - ModelTemperature.Properties.PbFactor1) / ModelTemperature.Properties.Factor1;
            Properties.TempPhi = fact2 * phio + pbfact;
            Properties.TempVbi = ModelParameters.Vt0 - ModelParameters.MosfetType *
                        (ModelParameters.Gamma * Math.Sqrt(ModelParameters.Phi))
                    + .5 * (ModelTemperature.Properties.EgFet1 - egfet)
                    + ModelParameters.MosfetType * .5 * (Properties.TempPhi - ModelParameters.Phi);
            Properties.TempVt0 = Properties.TempVbi + ModelParameters.MosfetType *
                    ModelParameters.Gamma * Math.Sqrt(Properties.TempPhi);
            Properties.TempSatCur = ModelParameters.JunctionSatCur *
                    Math.Exp(-egfet / Properties.TempVt + ModelTemperature.Properties.EgFet1 / ModelTemperature.Properties.Vtnom);
            Properties.TempSatCurDensity = ModelParameters.JunctionSatCurDensity *
                    Math.Exp(-egfet / Properties.TempVt + ModelTemperature.Properties.EgFet1 / ModelTemperature.Properties.Vtnom);
            double pbo = (ModelParameters.BulkJunctionPotential - ModelTemperature.Properties.PbFactor1) / ModelTemperature.Properties.Factor1;
            double gmaold = (ModelParameters.BulkJunctionPotential - pbo) / pbo;
            double capfact = 1 / (1 + ModelParameters.BulkJunctionBotGradingCoefficient *
                    (4e-4 * (ModelParameters.NominalTemperature - Constants.ReferenceTemperature) - gmaold));
            Properties.TempCbd = ModelParameters.CapBd * capfact;
            Properties.TempCbs = ModelParameters.CapBs * capfact;
            Properties.TempCj = ModelParameters.BulkCapFactor * capfact;
            capfact = 1 / (1 + ModelParameters.BulkJunctionSideGradingCoefficient *
                    (4e-4 * (ModelParameters.NominalTemperature - Constants.ReferenceTemperature) - gmaold));
            Properties.TempCjsw = ModelParameters.SidewallCapFactor * capfact;
            Properties.TempBulkPotential = fact2 * pbo + pbfact;
            double gmanew = (Properties.TempBulkPotential - pbo) / pbo;
            capfact = (1 + ModelParameters.BulkJunctionBotGradingCoefficient *
                    (4e-4 * (Parameters.Temperature - Constants.ReferenceTemperature) - gmanew));
            Properties.Cbd *= capfact;
            Properties.Cbs *= capfact;
            Properties.TempCj *= capfact;
            capfact = (1 + ModelParameters.BulkJunctionSideGradingCoefficient *
                    (4e-4 * (Parameters.Temperature - Constants.ReferenceTemperature) - gmanew));
            Properties.TempCjsw *= capfact;
            Properties.TempDepCap = ModelParameters.ForwardCapDepletionCoefficient * Properties.TempBulkPotential;

            if ((Properties.TempSatCurDensity == 0) || (Parameters.DrainArea == 0) || (Parameters.SourceArea == 0))
                Properties.SourceVCritical = Properties.DrainVCritical = Properties.TempVt * Math.Log(Properties.TempVt / (Constants.Root2 * Parameters.ParallelMultiplier * Properties.TempSatCur));
            else
            {
                Properties.DrainVCritical = Properties.TempVt * Math.Log(Properties.TempVt / (Constants.Root2 *
                        Parameters.ParallelMultiplier *
                        Properties.TempSatCurDensity * Parameters.DrainArea));
                Properties.SourceVCritical = Properties.TempVt * Math.Log(Properties.TempVt / (Constants.Root2 *
                        Parameters.ParallelMultiplier *
                        Properties.TempSatCurDensity * Parameters.SourceArea));
            }

            double czbd, czbdsw;
            if (ModelParameters.CapBd.Given)
                czbd = Properties.TempCbd * Parameters.ParallelMultiplier;
            else
            {
                if (ModelParameters.BulkCapFactor.Given)
                    czbd = Properties.TempCj * Parameters.DrainArea * Parameters.ParallelMultiplier;
                else
                    czbd = 0;
            }
            if (ModelParameters.SidewallCapFactor.Given)
                czbdsw = Properties.TempCjsw * Parameters.DrainPerimeter * Parameters.ParallelMultiplier;
            else
                czbdsw = 0;
            arg = 1 - ModelParameters.ForwardCapDepletionCoefficient;
            double sarg = Math.Exp((-ModelParameters.BulkJunctionBotGradingCoefficient) * Math.Log(arg));
            double sargsw = Math.Exp((-ModelParameters.BulkJunctionSideGradingCoefficient) * Math.Log(arg));
            Properties.Cbd = czbd;
            Properties.CbdSidewall = czbdsw;
            Properties.F2d = czbd * (1 - ModelParameters.ForwardCapDepletionCoefficient *
                        (1 + ModelParameters.BulkJunctionBotGradingCoefficient)) * sarg / arg
                    + czbdsw * (1 - ModelParameters.ForwardCapDepletionCoefficient *
                        (1 + ModelParameters.BulkJunctionSideGradingCoefficient)) *
                        sargsw / arg;
            Properties.F3d = czbd * ModelParameters.BulkJunctionBotGradingCoefficient * sarg / arg /
                        Properties.TempBulkPotential
                    + czbdsw * ModelParameters.BulkJunctionSideGradingCoefficient * sargsw / arg /
                        Properties.TempBulkPotential;
            Properties.F4d = czbd * Properties.TempBulkPotential * (1 - arg * sarg) /
                        (1 - ModelParameters.BulkJunctionBotGradingCoefficient)
                    + czbdsw * Properties.TempBulkPotential * (1 - arg * sargsw) /
                        (1 - ModelParameters.BulkJunctionSideGradingCoefficient)
                    - Properties.F3d / 2 * (Properties.TempDepCap * Properties.TempDepCap)
                    - Properties.TempDepCap * Properties.F2d;

            double czbs, czbssw;
            if (ModelParameters.CapBs.Given)
                czbs = Properties.TempCbs * Parameters.ParallelMultiplier;
            else
            {
                if (ModelParameters.BulkCapFactor.Given)
                    czbs = Properties.TempCj * Parameters.SourceArea * Parameters.ParallelMultiplier;
                else
                    czbs = 0;
            }
            if (ModelParameters.SidewallCapFactor.Given)
                czbssw = Properties.TempCjsw * Parameters.SourcePerimeter * Parameters.ParallelMultiplier;
            else
                czbssw = 0;
            arg = 1 - ModelParameters.ForwardCapDepletionCoefficient;
            sarg = Math.Exp((-ModelParameters.BulkJunctionBotGradingCoefficient) * Math.Log(arg));
            sargsw = Math.Exp((-ModelParameters.BulkJunctionSideGradingCoefficient) * Math.Log(arg));
            Properties.Cbs = czbs;
            Properties.CbsSidewall = czbssw;
            Properties.F2s = czbs * (1 - ModelParameters.ForwardCapDepletionCoefficient *
                        (1 + ModelParameters.BulkJunctionBotGradingCoefficient)) * sarg / arg
                    + czbssw * (1 - ModelParameters.ForwardCapDepletionCoefficient *
                        (1 + ModelParameters.BulkJunctionSideGradingCoefficient)) *
                        sargsw / arg;
            Properties.F3s = czbs * ModelParameters.BulkJunctionBotGradingCoefficient * sarg / arg /
                        Properties.TempBulkPotential
                    + czbssw * ModelParameters.BulkJunctionSideGradingCoefficient * sargsw / arg /
                        Properties.TempBulkPotential;
            Properties.F4s = czbs * Properties.TempBulkPotential * (1 - arg * sarg) /
                        (1 - ModelParameters.BulkJunctionBotGradingCoefficient)
                    + czbssw * Properties.TempBulkPotential * (1 - arg * sargsw) /
                        (1 - ModelParameters.BulkJunctionSideGradingCoefficient)
                    - Properties.F3s / 2 *
                        (Properties.TempDepCap * Properties.TempDepCap)
                    - Properties.TempDepCap * Properties.F2s;

            Properties.EffectiveLength = Parameters.Length - 2 * ModelParameters.LateralDiffusion;
            Properties.OxideCap = ModelTemperature.Properties.OxideCapFactor * Properties.EffectiveLength * Parameters.ParallelMultiplier * Parameters.Width;
        }
    }
}
