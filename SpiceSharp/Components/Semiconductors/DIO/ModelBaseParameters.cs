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
        public Parameter SaturationCurrent { get; } = new Parameter(1e-14);
        [ParameterName("tnom"), ParameterInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter NominalTemperature { get; } = new Parameter();
        [ParameterName("rs"), ParameterInfo("Ohmic resistance")]
        public Parameter Resistance { get; } = new Parameter();
        [ParameterName("n"), ParameterInfo("Emission Coefficient")]
        public Parameter EmissionCoefficient { get; } = new Parameter(1);
        [ParameterName("tt"), ParameterInfo("Transit Time")]
        public Parameter TransitTime { get; } = new Parameter();
        [ParameterName("cjo"), ParameterName("cj0"), ParameterInfo("Junction capacitance")]
        public Parameter JunctionCap { get; } = new Parameter();
        [ParameterName("vj"), ParameterInfo("Junction potential")]
        public Parameter JunctionPotential { get; } = new Parameter(1);
        [ParameterName("m"), ParameterInfo("Grading coefficient")]
        public Parameter GradingCoefficient { get; } = new Parameter(.5);
        [ParameterName("eg"), ParameterInfo("Activation energy")]
        public Parameter ActivationEnergy { get; } = new Parameter(1.11);
        [ParameterName("xti"), ParameterInfo("Saturation current temperature exp.")]
        public Parameter SaturationCurrentExp { get; } = new Parameter(3);
        [ParameterName("fc"), ParameterInfo("Forward bias junction fit parameter")]
        public Parameter DepletionCapCoefficient { get; } = new Parameter(.5);
        [ParameterName("bv"), ParameterInfo("Reverse breakdown voltage")]
        public Parameter BreakdownVoltage { get; } = new Parameter();
        [ParameterName("ibv"), ParameterInfo("Current at reverse breakdown voltage")]
        public Parameter BreakdownCurrent { get; } = new Parameter(1e-3);
    }
}
