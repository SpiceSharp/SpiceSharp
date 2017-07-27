using System;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Parameters
{
    public class ParameterMethod<T> : IParameter, ICloneable
    {
        /// <summary>
        /// A method that can transform an input if necessary
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public delegate T TransformMethod(T input);

        /// <summary>
        /// A transformation that is applied before returning the value using Get()
        /// </summary>
        public TransformMethod TransformGet = null;

        /// <summary>
        /// A transformation that is applied when setting the value using Set()
        /// </summary>
        public TransformMethod TransformSet = null;

        /// <summary>
        /// Gets or sets the raw value of the parameter without changing the Given parameter
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets whether or not the parameter was specified
        /// </summary>
        public bool Given { get; private set; } = false;

        /// <summary>
        /// Constructor with extra methods
        /// </summary>
        /// <param name="defvalue">The default value</param>
        /// <param name="set">The method used to convert the value when setting the parameter</param>
        /// <param name="get">The method used to convert the value when getting the parameter</param>
        public ParameterMethod(T defvalue, TransformMethod set, TransformMethod get)
        {
            Value = defvalue;
            TransformSet = set;
            TransformGet = get;
        }

        /// <summary>
        /// Clone the parameter
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var clone = new ParameterMethod<T>(Value, TransformSet, TransformGet)
            {
                Given = Given               
            };
            return clone;
        }

        /// <summary>
        /// Copy the parameter from another parameter
        /// </summary>
        /// <param name="source"></param>
        public void CopyFrom(ParameterMethod<T> source)
        {
            Value = source.Value;
            Given = source.Given;
        }

        /// <summary>
        /// Copy the parameter to another parameter
        /// </summary>
        /// <param name="target"></param>
        public void CopyTo(ParameterMethod<T> target)
        {
            target.Value = Value;
            target.Given = Given;
        }

        /// <summary>
        /// Specify the parameter
        /// </summary>
        /// <param name="value"></param>
        public void Set(T value)
        {
            if (TransformSet != null)
                Value = TransformSet(value);
            else
                Value = value;
            Given = true;
        }

        /// <summary>
        /// Get the value of the parameter
        /// </summary>
        /// <returns></returns>
        public object Get()
        {
            if (TransformGet != null)
                return TransformGet(Value);
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
                if (TransformSet != null)
                    Value = TransformSet((T)value);
                else
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
        public static implicit operator T(ParameterMethod<T> p)
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
