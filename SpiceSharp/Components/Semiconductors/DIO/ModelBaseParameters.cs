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
        [ParameterName("is"), PropertyInfo("Saturation current")]
        public Parameter SaturationCurrent { get; } = new Parameter(1e-14);
        [ParameterName("tnom"), PropertyInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter NominalTemperature { get; } = new Parameter();
        [ParameterName("rs"), PropertyInfo("Ohmic resistance")]
        public Parameter Resistance { get; } = new Parameter();
        [ParameterName("n"), PropertyInfo("Emission Coefficient")]
        public Parameter EmissionCoefficient { get; } = new Parameter(1);
        [ParameterName("tt"), PropertyInfo("Transit Time")]
        public Parameter TransitTime { get; } = new Parameter();
        [ParameterName("cjo"), ParameterName("cj0"), PropertyInfo("Junction capacitance")]
        public Parameter JunctionCap { get; } = new Parameter();
        [ParameterName("vj"), PropertyInfo("Junction potential")]
        public Parameter JunctionPotential { get; } = new Parameter(1);
        [ParameterName("m"), PropertyInfo("Grading coefficient")]
        public Parameter GradingCoefficient { get; } = new Parameter(.5);
        [ParameterName("eg"), PropertyInfo("Activation energy")]
        public Parameter ActivationEnergy { get; } = new Parameter(1.11);
        [ParameterName("xti"), PropertyInfo("Saturation current temperature exp.")]
        public Parameter SaturationCurrentExp { get; } = new Parameter(3);
        [ParameterName("fc"), PropertyInfo("Forward bias junction fit parameter")]
        public Parameter DepletionCapCoefficient { get; } = new Parameter(.5);
        [ParameterName("bv"), PropertyInfo("Reverse breakdown voltage")]
        public Parameter BreakdownVoltage { get; } = new Parameter();
        [ParameterName("ibv"), PropertyInfo("Current at reverse breakdown voltage")]
        public Parameter BreakdownCurrent { get; } = new Parameter(1e-3);
    }
}
