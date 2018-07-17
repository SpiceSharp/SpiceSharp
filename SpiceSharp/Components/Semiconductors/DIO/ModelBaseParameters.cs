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
        public GivenParameter SaturationCurrent { get; } = new GivenParameter(1e-14);
        [ParameterName("tnom"), DerivedProperty(), ParameterInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Value = value + Circuit.CelsiusKelvin;
        }
        public GivenParameter NominalTemperature { get; } = new GivenParameter(Circuit.ReferenceTemperature);
        [ParameterName("rs"), ParameterInfo("Ohmic resistance")]
        public GivenParameter Resistance { get; } = new GivenParameter();
        [ParameterName("n"), ParameterInfo("Emission Coefficient")]
        public GivenParameter EmissionCoefficient { get; } = new GivenParameter(1);
        [ParameterName("tt"), ParameterInfo("Transit Time")]
        public GivenParameter TransitTime { get; } = new GivenParameter();
        [ParameterName("cjo"), ParameterName("cj0"), ParameterInfo("Junction capacitance")]
        public GivenParameter JunctionCap { get; } = new GivenParameter();
        [ParameterName("vj"), ParameterInfo("Junction potential")]
        public GivenParameter JunctionPotential { get; } = new GivenParameter(1);
        [ParameterName("m"), ParameterInfo("Grading coefficient")]
        public GivenParameter GradingCoefficient { get; } = new GivenParameter(.5);
        [ParameterName("eg"), ParameterInfo("Activation energy")]
        public GivenParameter ActivationEnergy { get; } = new GivenParameter(1.11);
        [ParameterName("xti"), ParameterInfo("Saturation current temperature exp.")]
        public GivenParameter SaturationCurrentExp { get; } = new GivenParameter(3);
        [ParameterName("fc"), ParameterInfo("Forward bias junction fit parameter")]
        public GivenParameter DepletionCapCoefficient { get; } = new GivenParameter(.5);
        [ParameterName("bv"), ParameterInfo("Reverse breakdown voltage")]
        public GivenParameter BreakdownVoltage { get; } = new GivenParameter();
        [ParameterName("ibv"), ParameterInfo("Current at reverse breakdown voltage")]
        public GivenParameter BreakdownCurrent { get; } = new GivenParameter(1e-3);
    }
}
