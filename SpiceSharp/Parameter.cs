using System;

namespace SpiceSharp
{
    /// <summary>
    /// This class describes a parameter that is optional. Whether or not it was specified can be
    /// found using the Given variable.
    /// </summary>
    public class Parameter : ICloneable
    {
        /// <summary>
        /// Gets or sets the raw value of the parameter without changing the Given parameter
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets whether or not the parameter was specified
        /// </summary>
        public bool Given { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Parameter()
        {
            Value = 0.0;
            Given = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultValue">The default value</param>
        public Parameter(double defaultValue)
        {
            Value = defaultValue;
            Given = false;
        }

        /// <summary>
        /// Clone the parameter
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            var clone = new Parameter
            {
                Given = Given,
                Value = Value
            };
            return clone;
        }

        /// <summary>
        /// Copy the parameter from another parameter
        /// </summary>
        /// <param name="source">Copy from other parameters</param>
        public virtual void CopyFrom(Parameter source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Value = source.Value;
            Given = source.Given;
        }

        /// <summary>
        /// Copy the parameter to another parameter
        /// </summary>
        /// <param name="target">Target parameter</param>
        public virtual void CopyTo(Parameter target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            target.Value = Value;
            target.Given = Given;
        }

        /// <summary>
        /// Specify the parameter
        /// </summary>
        /// <param name="value"></param>
        public virtual void Set(double value)
        {
            Value = value;
            Given = true;
        }

        /// <summary>
        /// Parameters can be implicitly converted to their base type
        /// </summary>
        /// <param name="parameter"></param>
        public static implicit operator double(Parameter parameter)
        {
            if (parameter == null)
                return double.NaN;
            return parameter.Value;
        }

        /// <summary>
        /// Assignment
        /// Warning: This is the same as calling Set on the parameter!
        /// </summary>
        /// <param name="parameter">The double representation</param>
        public static implicit operator Parameter(double parameter)
        {
            return new Parameter(parameter) { Given = true };
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Given)
                return "{0} (set)".FormatString(Value);
            return "{0} (not set)".FormatString(Value);
        }
    }
}