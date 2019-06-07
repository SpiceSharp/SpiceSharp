using System;

namespace SpiceSharp
{
    /// <summary>
    /// A template for parameters of a specific type.
    /// </summary>
    /// <typeparam name="T">The base value type</typeparam>
    /// <seealso cref="BaseParameter" />
    public abstract class Parameter<T> : IDeepCloneable where T : struct
    {
        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <value>
        /// The value of the parameter.
        /// </value>
        public abstract T Value { get; set; }

        /// <summary>
        /// Copies the contents of a parameter to this parameter.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        /// <exception cref="CircuitException">Cannot copy: source is not a Parameter</exception>
        public virtual void CopyFrom(IDeepCloneable source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (source.GetType() == this.GetType())
                ParameterHelper.CopyPropertiesAndFields(source, this);
            else if (source is Parameter<T> p)
                Value = p.Value;
            else
                throw new CircuitException("Cannot copy: source is not a Parameter");
        }

        /// <summary>
        /// Clone the current parameter.
        /// </summary>
        /// <returns>A clone of the parameter.</returns>
        public abstract IDeepCloneable Clone();

        /// <summary>
        /// Performs an implicit conversion from <see cref="Parameter{T}"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator T(Parameter<T> parameter) => parameter?.Value ?? default(T);

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Parameter {0}".FormatString(Value);
        }
    }
}
