using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.Mosfet.Level1
{
    /// <summary>
    /// Base parameters for a <see cref="MOS1"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [NameAttribute("off"), InfoAttribute("Device initially off")]
        public bool MOS1off { get; set; }
        [NameAttribute("icvbs"), InfoAttribute("Initial B-S voltage")]
        public Parameter MOS1icVBS { get; } = new Parameter();
        [NameAttribute("icvds"), InfoAttribute("Initial D-S voltage")]
        public Parameter MOS1icVDS { get; } = new Parameter();
        [NameAttribute("icvgs"), InfoAttribute("Initial G-S voltage")]
        public Parameter MOS1icVGS { get; } = new Parameter();
        [NameAttribute("temp"), InfoAttribute("Instance temperature")]
        public double MOS1_TEMP
        {
            get => MOS1temp - Circuit.CONSTCtoK;
            set => MOS1temp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS1temp { get; } = new Parameter();
        [NameAttribute("w"), InfoAttribute("Width")]
        public Parameter MOS1w { get; } = new Parameter(1e-4);
        [NameAttribute("l"), InfoAttribute("Length")]
        public Parameter MOS1l { get; } = new Parameter(1e-4);
        [NameAttribute("as"), InfoAttribute("Source area")]
        public Parameter MOS1sourceArea { get; } = new Parameter();
        [NameAttribute("ad"), InfoAttribute("Drain area")]
        public Parameter MOS1drainArea { get; } = new Parameter();
        [NameAttribute("ps"), InfoAttribute("Source perimeter")]
        public Parameter MOS1sourcePerimiter { get; } = new Parameter();
        [NameAttribute("pd"), InfoAttribute("Drain perimeter")]
        public Parameter MOS1drainPerimiter { get; } = new Parameter();
        [NameAttribute("nrs"), InfoAttribute("Source squares")]
        public Parameter MOS1sourceSquares { get; } = new Parameter(1);
        [NameAttribute("nrd"), InfoAttribute("Drain squares")]
        public Parameter MOS1drainSquares { get; } = new Parameter(1);

        /// <summary>
        /// Methods
        /// </summary>
        [NameAttribute("ic"), InfoAttribute("Vector of D-S, G-S, B-S voltages")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 3: MOS1icVBS.Set(value[2]); goto case 2;
                case 2: MOS1icVGS.Set(value[1]); goto case 1;
                case 1: MOS1icVDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
    }
}
