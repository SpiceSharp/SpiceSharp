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
            set => NominalTemperature.Value = value + Circuit.CelsiusKelvin;
        }
        public GivenParameter NominalTemperature { get; } = new GivenParameter(300.15);
        [ParameterName("tc1"), ParameterInfo("First order temperature coefficient")]
        public GivenParameter TemperatureCoefficient1 { get; } = new GivenParameter();
        [ParameterName("tc2"), ParameterInfo("Second order temperature oefficient")]
        public GivenParameter TemperatureCoefficient2 { get; } = new GivenParameter();
        [ParameterName("rsh"), ParameterInfo("Sheet resistance")]
        public GivenParameter SheetResistance { get; } = new GivenParameter();
        [ParameterName("defw"), ParameterInfo("Default device width")]
        public GivenParameter DefaultWidth { get; } = new GivenParameter(10.0e-6);
        [ParameterName("narrow"), ParameterInfo("Narrowing of resistor")]
        public GivenParameter Narrow { get; } = new GivenParameter();
    }
}
