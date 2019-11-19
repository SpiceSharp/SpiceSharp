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
        public abstract T Set(string name);

        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The current instance for chaining.
        /// </returns>
        public abstract T Set<P>(string name, P value);

        /// <summary>
        /// Call a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        void IImportParameterSet.Set(string name) => Parameters.Set(name);

        /// <summary>
        /// Tries calling a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the method was called; otherwise <c>false</c>.
        /// </returns>
        public bool TrySet(string name) => Parameters.TrySet(name);

        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        void IImportParameterSet.Set<P>(string name, P value) => Parameters.Set(name, value);

        /// <summary>
        /// Tries to set the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter was set; otherwise <c>false</c>.
        /// </returns>
        public bool TrySet<P>(string name, P value) => Parameters.TrySet(name, value);

        /// <summary>
        /// Gets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public P Get<P>(string name) => Parameters.Get<P>(name);

        /// <summary>
        /// Tries to get the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGet<P>(string name, out P value) => Parameters.TryGet(name, out value);

        /// <summary>
        /// Creates a getter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A getter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        public Func<P> CreateGetter<P>(string name) => Parameters.CreateGetter<P>(name);

        /// <summary>
        /// Creates a setter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A setter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        public Action<P> CreateSetter<P>(string name) => Parameters.CreateSetter<P>(name);

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
