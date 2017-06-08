using System;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Parameters
{
    /// <summary>
    /// This class describes a parameter that is optional. Whether or not it is specified can be
    /// found using the Given variable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Parameter<T> : ICloneable, IParameter
    {
        /// <summary>
        /// Gets or sets the value of the parameter without changing the Given parameter
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets whether or not the parameter was specified
        /// </summary>
        public bool Given { get; private set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defvalue">The default value</param>
        public Parameter(T defvalue = default(T))
        {
            Value = defvalue;
        }

        /// <summary>
        /// Clone the parameter
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var clone = new Parameter<T>()
            {
                Given = Given,
                Value = Value
            };
            return clone;
        }

        /// <summary>
        /// Specify the parameter
        /// </summary>
        /// <param name="value"></param>
        public void Set(T value)
        {
            Value = value;
            Given = true;
        }

        /// <summary>
        /// Get the value of the parameter
        /// </summary>
        /// <returns></returns>
        public object Get()
        {
            return Value;
        }

        /// <summary>
        /// Set the value of the parameter
        /// </summary>
        /// <param name="value">The value</param>
        public void Set(object value)
        {
            if (value is T)
            {
                Value = (T)value;
                Given = true;
            }
            else
                throw new CircuitException($"Invalid type {value}");
        }

        /// <summary>
        /// Parameters can be implicitly converted to their base type
        /// </summary>
        /// <param name="p"></param>
        public static implicit operator T(Parameter<T> p)
        {
            return p.Value;
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
