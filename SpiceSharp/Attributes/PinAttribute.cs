using System;

namespace SpiceSharp.Attributes
{
    /// <summary>
    /// Specifies the pins for a circuit component that extends <see cref="Components.IComponent" />.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class PinAttribute : Attribute
    {
        /// <summary>
        /// The index of the pin.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The name of the pin.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PinAttribute"/> class.
        /// </summary>
        /// <param name="index">The index of the pin.</param>
        /// <param name="name">The name of the pin.</param>
        public PinAttribute(int index, string name)
        {
            Index = index;
            Name = name;
        }
    }
}
