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
        [NameAttribute("area"), InfoAttribute("Area factor")]
        public Parameter DIOarea { get; } = new Parameter(1);
        [NameAttribute("off"), InfoAttribute("Initially off")]
        public bool DIOoff { get; set; }
        [NameAttribute("ic"), InfoAttribute("Initial device voltage")]
        public double DIOinitCond { get; set; }
        [NameAttribute("sens_area"), InfoAttribute("flag to request sensitivity WRT area")]
        public bool DIOsenParmNo { get; set; }
        [NameAttribute("temp"), InfoAttribute("Instance temperature")]
        public double DIO_TEMP
        {
            get => DIOtemp - Circuit.CONSTCtoK;
            set => DIOtemp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter DIOtemp { get; } = new Parameter();
    }
}
