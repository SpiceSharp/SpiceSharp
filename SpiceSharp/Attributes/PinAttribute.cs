using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Specifies the pins for a circuit component that extends <see cref="Components.Component"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class PinAttribute : Attribute
    {
        /// <summary>
        /// Index of the pin
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Name of the pin
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="name">Name</param>
        public PinAttribute(int index, string name)
        {
            Index = index;
            Name = name;
        }
    }
}
