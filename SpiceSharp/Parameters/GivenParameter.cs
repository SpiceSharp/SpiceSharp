namespace SpiceSharp
{
    /// <summary>
    /// This class describes a parameter that is optional. Whether or not it was specified can be
    /// found using the Given variable.
    /// </summary>
    public class GivenParameter : Parameter
    {
        /// <summary>
        /// Gets or sets the value of the parameter
        /// </summary>
        public override double Value
        {
            get => RawValue;
            set
            {
                RawValue = value;
                Given = true;
            }
        }

        /// <summary>
        /// Gets or sets the raw value of the parameter without changing <see cref="Given"/>
        /// </summary>
        public double RawValue { get; set; }

        /// <summary>
        /// Gets whether or not the parameter was specified
        /// </summary>
        public bool Given { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GivenParameter()
        {
            RawValue = 0.0;
            Given = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultValue">The default value</param>
        public GivenParameter(double defaultValue)
        {
            RawValue = defaultValue;
            Given = false;
        }

        /// <summary>
        /// Clone the parameter
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            var clone = new GivenParameter
            {
                Given = Given,
                RawValue = RawValue
            };
            return clone;
        }

        /// <summary>
        /// Copy the parameter from another parameter
        /// </summary>
        /// <param name="source">Copy from other parameters</param>
        public override void CopyFrom(Parameter source)
        {
            if (source is GivenParameter gp)
            {
                RawValue = gp.RawValue;
                Given = gp.Given;
            }
            else
            {
                base.CopyFrom(source);
            }
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