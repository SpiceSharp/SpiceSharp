using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="DiodeModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("is"), ParameterInfo("Saturation current")]
        public GivenParameter<double> SaturationCurrent { get; } = new GivenParameter<double>(1e-14);
        [ParameterName("tnom"), DerivedProperty(), ParameterInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature.Value = value + Constants.CelsiusKelvin;
        }
        public GivenParameter<double> NominalTemperature { get; } = new GivenParameter<double>(Constants.ReferenceTemperature);
        [ParameterName("rs"), ParameterInfo("Ohmic resistance")]
        public GivenParameter<double> Resistance { get; } = new GivenParameter<double>();
        [ParameterName("n"), ParameterInfo("Emission Coefficient")]
        public GivenParameter<double> EmissionCoefficient { get; } = new GivenParameter<double>(1);
        [ParameterName("tt"), ParameterInfo("Transit Time")]
        public GivenParameter<double> TransitTime { get; } = new GivenParameter<double>();
        [ParameterName("cjo"), ParameterName("cj0"), ParameterInfo("Junction capacitance")]
        public GivenParameter<double> JunctionCap { get; } = new GivenParameter<double>();
        [ParameterName("vj"), ParameterInfo("Junction potential")]
        public GivenParameter<double> JunctionPotential { get; } = new GivenParameter<double>(1);
        [ParameterName("m"), ParameterInfo("Grading coefficient")]
        public GivenParameter<double> GradingCoefficient { get; } = new GivenParameter<double>(.5);
        [ParameterName("eg"), ParameterInfo("Activation energy")]
        public GivenParameter<double> ActivationEnergy { get; } = new GivenParameter<double>(1.11);
        [ParameterName("xti"), ParameterInfo("Saturation current temperature exp.")]
        public GivenParameter<double> SaturationCurrentExp { get; } = new GivenParameter<double>(3);
        [ParameterName("fc"), ParameterInfo("Forward bias junction fit parameter")]
        public GivenParameter<double> DepletionCapCoefficient { get; } = new GivenParameter<double>(.5);
        [ParameterName("bv"), ParameterInfo("Reverse breakdown voltage")]
        public GivenParameter<double> BreakdownVoltage { get; } = new GivenParameter<double>();
        [ParameterName("ibv"), ParameterInfo("Current at reverse breakdown voltage")]
        public GivenParameter<double> BreakdownCurrent { get; } = new GivenParameter<double>(1e-3);
    }
}
