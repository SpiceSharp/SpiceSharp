using System;

namespace SpiceSharp
{
    /// <summary>
    /// A base class for instances that require one or more <see cref="IParameterSet"/>.
    /// </summary>
    /// <seealso cref="IParameterSet" />
    public abstract class Parameterized<T> : IParameterized<T>
    {
        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public IParameterSetDictionary Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parameterized{T}"/> class.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        protected Parameterized(IParameterSetDictionary parameters)
        {
            Parameters = parameters.ThrowIfNull(nameof(parameters));
        }

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public void CalculateDefaults() => Parameters.CalculateDefaults();

        /// <summary>
        /// Call a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <returns>
        /// The current instance for chaining.
        /// </returns>
        public abstract T SetParameter(string name);

        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The current instance for chaining.
        /// </returns>
        public abstract T SetParameter<P>(string name, P value);

        /// <summary>
        /// Call a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        void IImportParameterSet.SetParameter(string name) => Parameters.SetParameter(name);

        /// <summary>
        /// Tries calling a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the method was called; otherwise <c>false</c>.
        /// </returns>
        public bool TrySetParameter(string name) => Parameters.TrySetParameter(name);

        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        void IImportParameterSet.SetParameter<P>(string name, P value) => Parameters.SetParameter(name, value);

        /// <summary>
        /// Tries to set the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter was set; otherwise <c>false</c>.
        /// </returns>
        public bool TrySetParameter<P>(string name, P value) => Parameters.TrySetParameter(name, value);

        /// <summary>
        /// Gets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public P GetProperty<P>(string name) => Parameters.GetProperty<P>(name);

        /// <summary>
        /// Tries to get the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetProperty<P>(string name, out P value) => Parameters.TryGetProperty(name, out value);

        /// <summary>
        /// Creates a getter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A getter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        public Func<P> CreatePropertyGetter<P>(string name) => Parameters.CreatePropertyGetter<P>(name);

        /// <summary>
        /// Creates a setter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A setter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        public Action<P> CreateParameterSetter<P>(string name) => Parameters.CreateParameterSetter<P>(name);

        /// <summary>
        /// Clones the entity
        /// </summary>
        /// <returns></returns>
        protected abstract ICloneable Clone();

        /// <summary>
        /// Clones this object.
        /// </summary>
        ICloneable ICloneable.Clone() => Clone();

        /// <summary>
        /// Copy properties from another entity.
        /// </summary>
        /// <param name="source">The source entity.</param>
        protected abstract void CopyFrom(ICloneable source);

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        void ICloneable.CopyFrom(ICloneable source) => CopyFrom(source);
    }
}
