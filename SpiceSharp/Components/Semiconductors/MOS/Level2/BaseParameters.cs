using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.Mosfet.Level2
{
    /// <summary>
    /// Base parameters for a <see cref="MOS2"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyNameAttribute("temp"), PropertyInfoAttribute("Instance operating temperature")]
        public double MOS2_TEMP
        {
            get => MOS2temp - Circuit.CONSTCtoK;
            set => MOS2temp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS2temp { get; } = new Parameter();
        [PropertyNameAttribute("w"), PropertyInfoAttribute("Width")]
        public Parameter MOS2w { get; } = new Parameter(1e-4);
        [PropertyNameAttribute("l"), PropertyInfoAttribute("Length")]
        public Parameter MOS2l { get; } = new Parameter(1e-4);
        [PropertyNameAttribute("as"), PropertyInfoAttribute("Source area")]
        public Parameter MOS2sourceArea { get; } = new Parameter();
        [PropertyNameAttribute("ad"), PropertyInfoAttribute("Drain area")]
        public Parameter MOS2drainArea { get; } = new Parameter();
        [PropertyNameAttribute("ps"), PropertyInfoAttribute("Source perimeter")]
        public Parameter MOS2sourcePerimiter { get; } = new Parameter();
        [PropertyNameAttribute("pd"), PropertyInfoAttribute("Drain perimeter")]
        public Parameter MOS2drainPerimiter { get; } = new Parameter();
        [PropertyNameAttribute("nrs"), PropertyInfoAttribute("Source squares")]
        public Parameter MOS2sourceSquares { get; } = new Parameter(1);
        [PropertyNameAttribute("nrd"), PropertyInfoAttribute("Drain squares")]
        public Parameter MOS2drainSquares { get; } = new Parameter(1);
        [PropertyNameAttribute("off"), PropertyInfoAttribute("Device initially off")]
        public bool MOS2off { get; set; }
        [PropertyNameAttribute("icvbs"), PropertyInfoAttribute("Initial B-S voltage")]
        public Parameter MOS2icVBS { get; } = new Parameter();
        [PropertyNameAttribute("icvds"), PropertyInfoAttribute("Initial D-S voltage")]
        public Parameter MOS2icVDS { get; } = new Parameter();
        [PropertyNameAttribute("icvgs"), PropertyInfoAttribute("Initial G-S voltage")]
        public Parameter MOS2icVGS { get; } = new Parameter();
        [PropertyNameAttribute("ic"), PropertyInfoAttribute("Vector of D-S, G-S, B-S voltages")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 3: MOS2icVBS.Set(value[2]); goto case 2;
                case 2: MOS2icVGS.Set(value[1]); goto case 1;
                case 1: MOS2icVDS.Set(value[0]); break;
                default:
                    throw new BadParameterException("ic");
            }
        }
    }
}
