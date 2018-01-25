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
        [PropertyNameAttribute("off"), PropertyInfoAttribute("Device initially off")]
        public bool MOS1off { get; set; }
        [PropertyNameAttribute("icvbs"), PropertyInfoAttribute("Initial B-S voltage")]
        public Parameter MOS1icVBS { get; } = new Parameter();
        [PropertyNameAttribute("icvds"), PropertyInfoAttribute("Initial D-S voltage")]
        public Parameter MOS1icVDS { get; } = new Parameter();
        [PropertyNameAttribute("icvgs"), PropertyInfoAttribute("Initial G-S voltage")]
        public Parameter MOS1icVGS { get; } = new Parameter();
        [PropertyNameAttribute("temp"), PropertyInfoAttribute("Instance temperature")]
        public double MOS1_TEMP
        {
            get => MOS1temp - Circuit.CONSTCtoK;
            set => MOS1temp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS1temp { get; } = new Parameter();
        [PropertyNameAttribute("w"), PropertyInfoAttribute("Width")]
        public Parameter MOS1w { get; } = new Parameter(1e-4);
        [PropertyNameAttribute("l"), PropertyInfoAttribute("Length")]
        public Parameter MOS1l { get; } = new Parameter(1e-4);
        [PropertyNameAttribute("as"), PropertyInfoAttribute("Source area")]
        public Parameter MOS1sourceArea { get; } = new Parameter();
        [PropertyNameAttribute("ad"), PropertyInfoAttribute("Drain area")]
        public Parameter MOS1drainArea { get; } = new Parameter();
        [PropertyNameAttribute("ps"), PropertyInfoAttribute("Source perimeter")]
        public Parameter MOS1sourcePerimiter { get; } = new Parameter();
        [PropertyNameAttribute("pd"), PropertyInfoAttribute("Drain perimeter")]
        public Parameter MOS1drainPerimiter { get; } = new Parameter();
        [PropertyNameAttribute("nrs"), PropertyInfoAttribute("Source squares")]
        public Parameter MOS1sourceSquares { get; } = new Parameter(1);
        [PropertyNameAttribute("nrd"), PropertyInfoAttribute("Drain squares")]
        public Parameter MOS1drainSquares { get; } = new Parameter(1);

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyNameAttribute("ic"), PropertyInfoAttribute("Vector of D-S, G-S, B-S voltages")]
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
