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
        [PropertyNameAttribute("w"), PropertyInfoAttribute("Width")]
        public Parameter MOS3w { get; } = new Parameter(1e-4);
        [PropertyNameAttribute("l"), PropertyInfoAttribute("Length")]
        public Parameter MOS3l { get; } = new Parameter(1e-4);
        [PropertyNameAttribute("as"), PropertyInfoAttribute("Source area")]
        public Parameter MOS3sourceArea { get; } = new Parameter();
        [PropertyNameAttribute("ad"), PropertyInfoAttribute("Drain area")]
        public Parameter MOS3drainArea { get; } = new Parameter();
        [PropertyNameAttribute("ps"), PropertyInfoAttribute("Source perimeter")]
        public Parameter MOS3sourcePerimiter { get; } = new Parameter();
        [PropertyNameAttribute("pd"), PropertyInfoAttribute("Drain perimeter")]
        public Parameter MOS3drainPerimiter { get; } = new Parameter();
        [PropertyNameAttribute("nrs"), PropertyInfoAttribute("Source squares")]
        public Parameter MOS3sourceSquares { get; } = new Parameter(1);
        [PropertyNameAttribute("nrd"), PropertyInfoAttribute("Drain squares")]
        public Parameter MOS3drainSquares { get; } = new Parameter(1);
        [PropertyNameAttribute("temp"), PropertyInfoAttribute("Instance operating temperature")]
        public double MOS3_TEMP
        {
            get => MOS3temp - Circuit.CONSTCtoK;
            set => MOS3temp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS3temp { get; } = new Parameter();

        [PropertyNameAttribute("off"), PropertyInfoAttribute("Device initially off")]
        public bool MOS3off { get; set; }
        [PropertyNameAttribute("icvbs"), PropertyInfoAttribute("Initial B-S voltage")]
        public Parameter MOS3icVBS { get; } = new Parameter();
        [PropertyNameAttribute("icvds"), PropertyInfoAttribute("Initial D-S voltage")]
        public Parameter MOS3icVDS { get; } = new Parameter();
        [PropertyNameAttribute("icvgs"), PropertyInfoAttribute("Initial G-S voltage")]
        public Parameter MOS3icVGS { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyNameAttribute("ic"), PropertyInfoAttribute("Vector of D-S, G-S, B-S voltages")]
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
