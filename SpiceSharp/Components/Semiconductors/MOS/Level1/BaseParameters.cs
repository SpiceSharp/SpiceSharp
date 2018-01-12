using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.Mosfet.Level1
{
    /// <summary>
    /// Base parameters for a <see cref="MOS1"/>
    /// </summary>
    public class BaseParameters : Parameters
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("off"), SpiceInfo("Device initially off")]
        public bool MOS1off { get; set; }
        [SpiceName("icvbs"), SpiceInfo("Initial B-S voltage")]
        public Parameter MOS1icVBS { get; } = new Parameter();
        [SpiceName("icvds"), SpiceInfo("Initial D-S voltage")]
        public Parameter MOS1icVDS { get; } = new Parameter();
        [SpiceName("icvgs"), SpiceInfo("Initial G-S voltage")]
        public Parameter MOS1icVGS { get; } = new Parameter();
        [SpiceName("temp"), SpiceInfo("Instance temperature")]
        public double MOS1_TEMP
        {
            get => MOS1temp - Circuit.CONSTCtoK;
            set => MOS1temp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS1temp { get; } = new Parameter();
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter MOS1w { get; } = new Parameter(1e-4);
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter MOS1l { get; } = new Parameter(1e-4);
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter MOS1sourceArea { get; } = new Parameter();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter MOS1drainArea { get; } = new Parameter();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter MOS1sourcePerimiter { get; } = new Parameter();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter MOS1drainPerimiter { get; } = new Parameter();
        [SpiceName("nrs"), SpiceInfo("Source squares")]
        public Parameter MOS1sourceSquares { get; } = new Parameter(1);
        [SpiceName("nrd"), SpiceInfo("Drain squares")]
        public Parameter MOS1drainSquares { get; } = new Parameter(1);

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("ic"), SpiceInfo("Vector of D-S, G-S, B-S voltages")]
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
