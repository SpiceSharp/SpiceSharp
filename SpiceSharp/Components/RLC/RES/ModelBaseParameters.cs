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
        [ParameterName("tnom"), PropertyInfo("Parameter measurement temperature", Interesting = false)]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter NominalTemperature { get; } = new Parameter(300.15);
        [ParameterName("tc1"), PropertyInfo("First order temperature coefficient")]
        public Parameter TemperatureCoefficient1 { get; } = new Parameter();
        [ParameterName("tc2"), PropertyInfo("Second order temperature oefficient")]
        public Parameter TemperatureCoefficient2 { get; } = new Parameter();
        [ParameterName("rsh"), PropertyInfo("Sheet resistance")]
        public Parameter SheetResistance { get; } = new Parameter();
        [ParameterName("defw"), PropertyInfo("Default device width")]
        public Parameter DefaultWidth { get; } = new Parameter(10.0e-6);
        [ParameterName("narrow"), PropertyInfo("Narrowing of resistor")]
        public Parameter Narrow { get; } = new Parameter();
    }
}
