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
        [NameAttribute("resistance"), InfoAttribute("Resistance", IsPrincipal = true)]
        public Parameter RESresist { get; } = new Parameter();
        [NameAttribute("temp"), InfoAttribute("Instance operating temperature", Interesting = false)]
        public double RES_TEMP
        {
            get => REStemp - Circuit.CONSTCtoK;
            set => REStemp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter REStemp { get; } = new Parameter(300.15);
        [NameAttribute("w"), InfoAttribute("Width", Interesting = false)]
        public Parameter RESwidth { get; } = new Parameter();
        [NameAttribute("l"), InfoAttribute("Length", Interesting = false)]
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
