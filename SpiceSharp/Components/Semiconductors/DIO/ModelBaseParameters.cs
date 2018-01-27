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
        [PropertyName("is"), PropertyInfo("Saturation current")]
        public Parameter SaturationCurrent { get; } = new Parameter(1e-14);
        [PropertyName("tnom"), PropertyInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter NominalTemperature { get; } = new Parameter();
        [PropertyName("rs"), PropertyInfo("Ohmic resistance")]
        public Parameter Resistance { get; } = new Parameter();
        [PropertyName("n"), PropertyInfo("Emission Coefficient")]
        public Parameter EmissionCoefficient { get; } = new Parameter(1);
        [PropertyName("tt"), PropertyInfo("Transit Time")]
        public Parameter TransitTime { get; } = new Parameter();
        [PropertyName("cjo"), PropertyName("cj0"), PropertyInfo("Junction capacitance")]
        public Parameter JunctionCap { get; } = new Parameter();
        [PropertyName("vj"), PropertyInfo("Junction potential")]
        public Parameter JunctionPotential { get; } = new Parameter(1);
        [PropertyName("m"), PropertyInfo("Grading coefficient")]
        public Parameter GradingCoefficient { get; } = new Parameter(.5);
        [PropertyName("eg"), PropertyInfo("Activation energy")]
        public Parameter ActivationEnergy { get; } = new Parameter(1.11);
        [PropertyName("xti"), PropertyInfo("Saturation current temperature exp.")]
        public Parameter SaturationCurrentExp { get; } = new Parameter(3);
        [PropertyName("fc"), PropertyInfo("Forward bias junction fit parameter")]
        public Parameter DepletionCapCoefficient { get; } = new Parameter(.5);
        [PropertyName("bv"), PropertyInfo("Reverse breakdown voltage")]
        public Parameter BreakdownVoltage { get; } = new Parameter();
        [PropertyName("ibv"), PropertyInfo("Current at reverse breakdown voltage")]
        public Parameter BreakdownCurrent { get; } = new Parameter(1e-3);
    }
}
