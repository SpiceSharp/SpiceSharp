using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// An attribute that can be used to flag an interface as a behavior
    /// that is relevant for simulations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class SimulationBehaviorAttribute : Attribute
    {
    }
}
