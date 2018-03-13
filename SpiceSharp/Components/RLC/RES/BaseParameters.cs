using SpiceSharp.Attributes;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Base set of parameters for a <see cref="Resistor"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("resistance"), PropertyInfo("Resistance", IsPrincipal = true)]
        public Parameter Resistance { get; } = new Parameter();
        [ParameterName("temp"), PropertyInfo("Instance operating temperature", Interesting = false)]
        public double TemperatureCelsius
        {
            get => Temperature - Circuit.CelsiusKelvin;
            set => Temperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter Temperature { get; } = new Parameter(300.15);
        [ParameterName("w"), PropertyInfo("Width", Interesting = false)]
        public Parameter Width { get; } = new Parameter();
        [ParameterName("l"), PropertyInfo("Length", Interesting = false)]
        public Parameter Length { get; } = new Parameter();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="res">Resistor</param>
        public BaseParameters(double res)
        {
            Resistance.Set(res);
        }
    }
}
