using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Parameters for a <see cref="Delay"/>
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        [ParameterName("td"), ParameterName("delay"), ParameterInfo("The delay in seconds", IsPrincipal = true)]
        public GivenParameter<double> TimeDelay { get; } = new GivenParameter<double>();
    }
}
