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
        [PropertyName("resistance"), PropertyInfo("Resistance", IsPrincipal = true)]
        public Parameter Resistance { get; } = new Parameter();
        [PropertyName("temp"), PropertyInfo("Instance operating temperature", Interesting = false)]
        public double RES_TEMP
        {
            get => Temperature - Circuit.CelsiusKelvin;
            set => Temperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter Temperature { get; } = new Parameter(300.15);
        [PropertyName("w"), PropertyInfo("Width", Interesting = false)]
        public Parameter Width { get; } = new Parameter();
        [PropertyName("l"), PropertyInfo("Length", Interesting = false)]
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
