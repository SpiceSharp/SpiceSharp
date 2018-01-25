using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.Bipolar
{
    /// <summary>
    /// Base parameters for a <see cref="BJT"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("area"), PropertyInfo("Area factor")]
        public Parameter BJTarea { get; } = new Parameter(1);
        [PropertyName("off"), PropertyInfo("Device initially off")]
        public bool BJToff { get; set; }
        [PropertyName("icvbe"), PropertyInfo("Initial B-E voltage")]
        public Parameter BJTicVBE { get; } = new Parameter();
        [PropertyName("icvce"), PropertyInfo("Initial C-E voltage")]
        public Parameter BJTicVCE { get; } = new Parameter();
        [PropertyName("sens_area"), PropertyInfo("flag to request sensitivity WRT area")]
        public bool BJTsenParmNo { get; set; }

        [PropertyName("ic"), PropertyInfo("Initial condition vector")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 2: BJTicVCE.Set(value[1]); goto case 1;
                case 1: BJTicVBE.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
    }
}
