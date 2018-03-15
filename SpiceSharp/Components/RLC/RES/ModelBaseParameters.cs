using SpiceSharp.Attributes;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Parameters for a <see cref="ResistorModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("tnom"), ParameterInfo("Parameter measurement temperature", Interesting = false)]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter NominalTemperature { get; } = new Parameter(300.15);
        [ParameterName("tc1"), ParameterInfo("First order temperature coefficient")]
        public Parameter TemperatureCoefficient1 { get; } = new Parameter();
        [ParameterName("tc2"), ParameterInfo("Second order temperature oefficient")]
        public Parameter TemperatureCoefficient2 { get; } = new Parameter();
        [ParameterName("rsh"), ParameterInfo("Sheet resistance")]
        public Parameter SheetResistance { get; } = new Parameter();
        [ParameterName("defw"), ParameterInfo("Default device width")]
        public Parameter DefaultWidth { get; } = new Parameter(10.0e-6);
        [ParameterName("narrow"), ParameterInfo("Narrowing of resistor")]
        public Parameter Narrow { get; } = new Parameter();
    }
}
