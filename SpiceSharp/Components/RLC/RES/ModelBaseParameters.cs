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
        [ParameterName("tnom"), DerivedProperty(), ParameterInfo("Parameter measurement temperature", Interesting = false)]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature.Value = value + Constants.CelsiusKelvin;
        }
        public GivenParameter<double> NominalTemperature { get; } = new GivenParameter<double>(Constants.ReferenceTemperature);
        [ParameterName("tc1"), ParameterInfo("First order temperature coefficient")]
        public GivenParameter<double> TemperatureCoefficient1 { get; } = new GivenParameter<double>();
        [ParameterName("tc2"), ParameterInfo("Second order temperature coefficient")]
        public GivenParameter<double> TemperatureCoefficient2 { get; } = new GivenParameter<double>();
        [ParameterName("tce"), ParameterInfo("Exponential temperature coefficient")]
        public GivenParameter<double> ExponentialCoefficient { get; } = new GivenParameter<double>();
        [ParameterName("rsh"), ParameterInfo("Sheet resistance")]
        public GivenParameter<double> SheetResistance { get; } = new GivenParameter<double>();
        [ParameterName("defw"), ParameterInfo("Default device width")]
        public GivenParameter<double> DefaultWidth { get; } = new GivenParameter<double>(10.0e-6);
        [ParameterName("narrow"), ParameterInfo("Narrowing of resistor")]
        public GivenParameter<double> Narrow { get; } = new GivenParameter<double>();
    }
}
