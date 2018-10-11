using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Base parameters for a
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class ModelBaseParameters : ParameterSet
    {
        [ParameterName("reltol"), ParameterInfo("The relative tolerance used to decide whether or not to add a breakpoint.")]
        public double RelativeTolerance { get; set; }

        [ParameterName("abstol"), ParameterInfo("The absolute tolernace used to decide whether or not to add a breakpoint.")]
        public double AbsoluteTolerance { get; set; }
    }
}
