using SpiceSharp.Attributes;
using System;

// Due to lack of a better place, we add the assembly attribute here
[assembly:Behavioral]

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// An attribute that indicates that the assembly contains behaviors that can be 
    /// used by Spice#.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class BehavioralAttribute : Attribute
    {
    }
}
