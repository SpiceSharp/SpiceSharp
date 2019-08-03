using System;

namespace SpiceSharp
{
    /// <summary>
    /// A template for parameters of a specific type.
    /// </summary>
    /// <typeparam name="T">The base value type</typeparam>
    public abstract class Parameter<T> : ICloneable, ICloneable<Parameter<T>>
    {
        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        public abstract T Value { get; set; }

        /// <summary>
        /// Copies the contents of a parameter to this parameter.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        public virtual void CopyFrom(Parameter<T> source)
        {
            source.ThrowIfNull(nameof(source));

            if (source.GetType() == GetType())
                Reflection.CopyPropertiesAndFields(source, this);
            else if (source is Parameter<T> p)
                Value = p.Value;
            else
                throw new CircuitException("Cannot copy: source is not a Parameter");
        }

        /// <summary>
        /// Copies the contents of an object to this object.
        /// </summary>
        /// <param name="source">The source object.</param>
        void ICloneable.CopyFrom(ICloneable source) => CopyFrom((Parameter<T>)source);

        /// <summary>
        /// Clone the current parameter.
        /// </summary>
        /// <returns>A clone of the parameter.</returns>
        public virtual Parameter<T> Clone()
        {
            var clone = (Parameter<T>)Activator.CreateInstance(GetType());
            clone.CopyFrom(this);
            return clone;
        }

        /// <summary>
        /// Clone the current object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        ICloneable ICloneable.Clone() => Clone();

        /// <summary>
        /// Performs an implicit conversion from <see cref="Parameter{T}"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator T(Parameter<T> parameter)
        {
            if (parameter == null)
                return default;
            return parameter.Value;
        }

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
