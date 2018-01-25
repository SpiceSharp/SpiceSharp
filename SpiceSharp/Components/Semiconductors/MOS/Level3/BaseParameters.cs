using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.Mosfet.Level3
{
    /// <summary>
    /// Base parameters for a <see cref="MOS3"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [NameAttribute("w"), InfoAttribute("Width")]
        public Parameter MOS3w { get; } = new Parameter(1e-4);
        [NameAttribute("l"), InfoAttribute("Length")]
        public Parameter MOS3l { get; } = new Parameter(1e-4);
        [NameAttribute("as"), InfoAttribute("Source area")]
        public Parameter MOS3sourceArea { get; } = new Parameter();
        [NameAttribute("ad"), InfoAttribute("Drain area")]
        public Parameter MOS3drainArea { get; } = new Parameter();
        [NameAttribute("ps"), InfoAttribute("Source perimeter")]
        public Parameter MOS3sourcePerimiter { get; } = new Parameter();
        [NameAttribute("pd"), InfoAttribute("Drain perimeter")]
        public Parameter MOS3drainPerimiter { get; } = new Parameter();
        [NameAttribute("nrs"), InfoAttribute("Source squares")]
        public Parameter MOS3sourceSquares { get; } = new Parameter(1);
        [NameAttribute("nrd"), InfoAttribute("Drain squares")]
        public Parameter MOS3drainSquares { get; } = new Parameter(1);
        [NameAttribute("temp"), InfoAttribute("Instance operating temperature")]
        public double MOS3_TEMP
        {
            get => MOS3temp - Circuit.CONSTCtoK;
            set => MOS3temp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS3temp { get; } = new Parameter();

        [NameAttribute("off"), InfoAttribute("Device initially off")]
        public bool MOS3off { get; set; }
        [NameAttribute("icvbs"), InfoAttribute("Initial B-S voltage")]
        public Parameter MOS3icVBS { get; } = new Parameter();
        [NameAttribute("icvds"), InfoAttribute("Initial D-S voltage")]
        public Parameter MOS3icVDS { get; } = new Parameter();
        [NameAttribute("icvgs"), InfoAttribute("Initial G-S voltage")]
        public Parameter MOS3icVGS { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [NameAttribute("ic"), InfoAttribute("Vector of D-S, G-S, B-S voltages")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 3: MOS3icVBS.Set(value[2]); goto case 2;
                case 2: MOS3icVGS.Set(value[1]); goto case 1;
                case 1: MOS3icVDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
    }
}
