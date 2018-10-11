using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DelayBehaviors
{
    public class BaseParameters : ParameterSet
    {
        [ParameterName("delay"), ParameterName("td"), ParameterInfo("The delay.")]
        public double Delay { get; set; }
    }
}
