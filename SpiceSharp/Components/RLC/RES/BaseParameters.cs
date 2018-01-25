using SpiceSharp.Attributes;

namespace SpiceSharp.Components.RES
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
        public Parameter RESresist { get; } = new Parameter();
        [PropertyName("temp"), PropertyInfo("Instance operating temperature", Interesting = false)]
        public double RES_TEMP
        {
            get => REStemp - Circuit.CelsiusKelvin;
            set => REStemp.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter REStemp { get; } = new Parameter(300.15);
        [PropertyName("w"), PropertyInfo("Width", Interesting = false)]
        public Parameter RESwidth { get; } = new Parameter();
        [PropertyName("l"), PropertyInfo("Length", Interesting = false)]
        public Parameter RESlength { get; } = new Parameter();

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
            RESresist.Set(res);
        }
    }
}
