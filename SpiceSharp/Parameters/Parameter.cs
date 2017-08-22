using System;

namespace SpiceSharp.Parameters
{
    /// <summary>
    /// This struct describes a parameter that is optional. Whether or not it was specified can be
    /// found using the Given variable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
        /// <param name="defvalue">The default value</param>
        public Parameter(double defvalue = 0.0)
        {
            Value = defvalue;
            Given = false;
        }

        /// <summary>
        /// Clone the parameter
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var clone = new Parameter()
            {
                Given = Given,
                Value = Value
            };
            return clone;
        }

        /// <summary>
        /// Copy the parameter from another parameter
        /// </summary>
        /// <param name="source"></param>
        public void CopyFrom(Parameter source)
        {
            Value = source.Value;
            Given = source.Given;
        }

        /// <summary>
        /// Copy the parameter to another parameter
        /// </summary>
        /// <param name="target"></param>
        public void CopyTo(Parameter target)
        {
            target.Value = Value;
            target.Given = Given;
        }

        /// <summary>
        /// Specify the parameter
        /// </summary>
        /// <param name="value"></param>
        public void Set(double value)
        {
            Value = value;
            Given = true;
        }

        /// <summary>
        /// Parameters can be implicitly converted to their base type
        /// </summary>
        /// <param name="p"></param>
        public static implicit operator double(Parameter p)
        {
            return p.Value;
        }

        /// <summary>
        /// Assignment
        /// Warning: This is the same as calling Set on the parameter!
        /// </summary>
        /// <param name="p">The double representation</param>
        public static implicit operator Parameter(double p)
        {
            return new Parameter(p) { Given = true };
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Given)
                return Value.ToString() + " (set)";
            return Value.ToString();
        }
    }
}
