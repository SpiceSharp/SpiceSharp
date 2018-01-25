using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DIO
{
    /// <summary>
    /// Base parameters for a <see cref="Diode"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyNameAttribute("area"), PropertyInfoAttribute("Area factor")]
        public Parameter DIOarea { get; } = new Parameter(1);
        [PropertyNameAttribute("off"), PropertyInfoAttribute("Initially off")]
        public bool DIOoff { get; set; }
        [PropertyNameAttribute("ic"), PropertyInfoAttribute("Initial device voltage")]
        public double DIOinitCond { get; set; }
        [PropertyNameAttribute("sens_area"), PropertyInfoAttribute("flag to request sensitivity WRT area")]
        public bool DIOsenParmNo { get; set; }
        [PropertyNameAttribute("temp"), PropertyInfoAttribute("Instance temperature")]
        public double DIO_TEMP
        {
            get => DIOtemp - Circuit.CONSTCtoK;
            set => DIOtemp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter DIOtemp { get; } = new Parameter();
    }
}
