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
        [PropertyName("w"), PropertyInfo("Width")]
        public Parameter MOS3w { get; } = new Parameter(1e-4);
        [PropertyName("l"), PropertyInfo("Length")]
        public Parameter MOS3l { get; } = new Parameter(1e-4);
        [PropertyName("as"), PropertyInfo("Source area")]
        public Parameter MOS3sourceArea { get; } = new Parameter();
        [PropertyName("ad"), PropertyInfo("Drain area")]
        public Parameter MOS3drainArea { get; } = new Parameter();
        [PropertyName("ps"), PropertyInfo("Source perimeter")]
        public Parameter MOS3sourcePerimiter { get; } = new Parameter();
        [PropertyName("pd"), PropertyInfo("Drain perimeter")]
        public Parameter MOS3drainPerimiter { get; } = new Parameter();
        [PropertyName("nrs"), PropertyInfo("Source squares")]
        public Parameter MOS3sourceSquares { get; } = new Parameter(1);
        [PropertyName("nrd"), PropertyInfo("Drain squares")]
        public Parameter MOS3drainSquares { get; } = new Parameter(1);
        [PropertyName("temp"), PropertyInfo("Instance operating temperature")]
        public double MOS3_TEMP
        {
            get => MOS3temp - Circuit.CelsiusKelvin;
            set => MOS3temp.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter MOS3temp { get; } = new Parameter();

        [PropertyName("off"), PropertyInfo("Device initially off")]
        public bool MOS3off { get; set; }
        [PropertyName("icvbs"), PropertyInfo("Initial B-S voltage")]
        public Parameter MOS3icVBS { get; } = new Parameter();
        [PropertyName("icvds"), PropertyInfo("Initial D-S voltage")]
        public Parameter MOS3icVDS { get; } = new Parameter();
        [PropertyName("icvgs"), PropertyInfo("Initial G-S voltage")]
        public Parameter MOS3icVGS { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyName("ic"), PropertyInfo("Vector of D-S, G-S, B-S voltages")]
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
