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
        [NameAttribute("temp"), InfoAttribute("Instance operating temperature")]
        public double MOS2_TEMP
        {
            get => MOS2temp - Circuit.CONSTCtoK;
            set => MOS2temp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS2temp { get; } = new Parameter();
        [NameAttribute("w"), InfoAttribute("Width")]
        public Parameter MOS2w { get; } = new Parameter(1e-4);
        [NameAttribute("l"), InfoAttribute("Length")]
        public Parameter MOS2l { get; } = new Parameter(1e-4);
        [NameAttribute("as"), InfoAttribute("Source area")]
        public Parameter MOS2sourceArea { get; } = new Parameter();
        [NameAttribute("ad"), InfoAttribute("Drain area")]
        public Parameter MOS2drainArea { get; } = new Parameter();
        [NameAttribute("ps"), InfoAttribute("Source perimeter")]
        public Parameter MOS2sourcePerimiter { get; } = new Parameter();
        [NameAttribute("pd"), InfoAttribute("Drain perimeter")]
        public Parameter MOS2drainPerimiter { get; } = new Parameter();
        [NameAttribute("nrs"), InfoAttribute("Source squares")]
        public Parameter MOS2sourceSquares { get; } = new Parameter(1);
        [NameAttribute("nrd"), InfoAttribute("Drain squares")]
        public Parameter MOS2drainSquares { get; } = new Parameter(1);
        [NameAttribute("off"), InfoAttribute("Device initially off")]
        public bool MOS2off { get; set; }
        [NameAttribute("icvbs"), InfoAttribute("Initial B-S voltage")]
        public Parameter MOS2icVBS { get; } = new Parameter();
        [NameAttribute("icvds"), InfoAttribute("Initial D-S voltage")]
        public Parameter MOS2icVDS { get; } = new Parameter();
        [NameAttribute("icvgs"), InfoAttribute("Initial G-S voltage")]
        public Parameter MOS2icVGS { get; } = new Parameter();
        [NameAttribute("ic"), InfoAttribute("Vector of D-S, G-S, B-S voltages")]
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
