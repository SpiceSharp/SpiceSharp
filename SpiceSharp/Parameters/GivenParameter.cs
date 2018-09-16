namespace SpiceSharp
{
    /// <summary>
    /// This class describes a parameter that is optional. Whether or not it was specified can be
    /// found using the Given variable.
    /// </summary>
    public class GivenParameter<T> : Parameter<T> 
    {
        /// <summary>
        /// Gets or sets the value of the parameter
        /// </summary>
        public override T Value
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
        public T RawValue { get; set; }

        /// <summary>
        /// Gets whether or not the parameter was specified
        /// </summary>
        public bool Given { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GivenParameter()
        {
            RawValue = default(T);
            Given = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultValue">The default value</param>
        public GivenParameter(T defaultValue)
        {
            RawValue = defaultValue;
            Given = false;
        }

        /// <summary>
        /// Clone the parameter
        /// </summary>
        /// <returns></returns>
        public override BaseParameter Clone()
        {
            var clone = new GivenParameter<T>
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
        public override void CopyFrom(BaseParameter source)
        {
            if (source is GivenParameter<T> gp)
            {
                RawValue = gp.RawValue;
                Given = gp.Given;
            }
            else
                base.CopyFrom(source);
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