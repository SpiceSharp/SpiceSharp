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
        [PropertyName("area"), PropertyInfo("Area factor")]
        public Parameter DIOarea { get; } = new Parameter(1);
        [PropertyName("off"), PropertyInfo("Initially off")]
        public bool DIOoff { get; set; }
        [PropertyName("ic"), PropertyInfo("Initial device voltage")]
        public double DIOinitCond { get; set; }
        [PropertyName("sens_area"), PropertyInfo("flag to request sensitivity WRT area")]
        public bool DIOsenParmNo { get; set; }
        [PropertyName("temp"), PropertyInfo("Instance temperature")]
        public double DIO_TEMP
        {
            get => DIOtemp - Circuit.CONSTCtoK;
            set => DIOtemp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter DIOtemp { get; } = new Parameter();
    }
}
