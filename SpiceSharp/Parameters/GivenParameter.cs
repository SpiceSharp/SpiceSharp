namespace SpiceSharp
{
    /// <summary>
    /// This class describes a parameter that is optional. Whether or not it was specified can be
    /// found using the Given variable. It also has a default value when not specified.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="SpiceSharp.Parameter{T}" />
    public class GivenParameter<T> : Parameter<T>
    {
        /// <summary>
        /// Gets or sets the value of the parameter.
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
        /// Gets or sets the raw value of the parameter without changing <see cref="Given" />.
        /// </summary>
        public T RawValue { get; set; }

        /// <summary>
        /// Gets whether or not the parameter was specified by the user.
        /// </summary>
        public bool Given { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GivenParameter{T}"/> class.
        /// </summary>
        public GivenParameter()
        {
            RawValue = default;
            Given = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GivenParameter{T}"/> class.
        /// </summary>
        /// <param name="defaultValue">The default value when the parameter is not specified.</param>
        public GivenParameter(T defaultValue)
        {
            RawValue = defaultValue;
            Given = false;
        }

        /// <summary>
        /// Clones the parameter.
        /// </summary>
        /// <returns>
        /// The cloned parameter.
        /// </returns>
        public override Parameter<T> Clone()
        {
            var clone = new GivenParameter<T>
            {
                Given = Given,
                RawValue = RawValue
            };
            return clone;
        }

        /// <summary>
        /// Copies the contents of a parameter to this parameter.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        public override void CopyFrom(Parameter<T> source)
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
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (Given)
                return "{0} (set)".FormatString(Value);
            return "{0} (not set)".FormatString(Value);
        }
    }
}