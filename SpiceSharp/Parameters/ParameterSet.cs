using System;
using SpiceSharp.Attributes;

namespace SpiceSharp
{
    /// <summary>
    /// Base class for a set of parameters.
    /// </summary>
    /// <remarks>
    /// This class allows accessing parameters by their metadata. Metadata is specified by using 
    /// the <see cref="ParameterNameAttribute"/> and <see cref="ParameterInfoAttribute"/>.
    /// </remarks>
    public abstract class ParameterSet : 
        IChainableImportParameterSet<ParameterSet>, IParameterSet, ICloneable
    {
        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public virtual void CalculateDefaults()
        {
        }

        /// <summary>
        /// Creates a clone of the parameter set.
        /// </summary>
        /// <returns>
        /// A clone of the parameter set.
        /// </returns>
        protected virtual ICloneable Clone()
        {
            var clone = (ParameterSet) Activator.CreateInstance(GetType());
            clone.CopyFrom(this);
            return clone;
        }

        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        ICloneable ICloneable.Clone() => Clone();

        /// <summary>
        /// Copy properties and fields from another parameter set.
        /// </summary>
        /// <param name="source">The source parameter set.</param>
        protected virtual void CopyFrom(ICloneable source)
        {
            source.ThrowIfNull(nameof(source));
            Reflection.CopyPropertiesAndFields(source, this);
        }

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        void ICloneable.CopyFrom(ICloneable source) => CopyFrom(source);

        /// <summary>
        /// Call a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <returns>
        /// The current instance for chaining.
        /// </returns>
        public ParameterSet Set(string name)
        {
            Reflection.Set(this, name);
            return this;
        }

        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The current instance for chaining.
        /// </returns>
        public ParameterSet Set<P>(string name, P value)
        {
            Reflection.Set(this, name, value);
            return this;
        }

        /// <summary>
        /// Call a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        void IImportParameterSet.Set(string name) => Set(name);

        /// <summary>
        /// Tries calling a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the method was called; otherwise <c>false</c>.
        /// </returns>
        public bool TrySet(string name)
            => Reflection.TrySet(this, name);

        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        void IImportParameterSet.Set<P>(string name, P value) => Set(name, value);

        /// <summary>
        /// Tries to set the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter was set; otherwise <c>false</c>.
        /// </returns>
        public bool TrySet<P>(string name, P value)
            => Reflection.TrySet(this, name, value);

        /// <summary>
        /// Gets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public P Get<P>(string name)
            => Reflection.Get<P>(this, name);

        /// <summary>
        /// Tries to get the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGet<P>(string name, out P value)
            => Reflection.TryGet(this, name, out value);

        /// <summary>
        /// Creates a getter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A getter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        public Func<P> CreateGetter<P>(string name)
            => Reflection.CreateGetter<P>(this, name);

        /// <summary>
        /// Creates a setter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A setter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        public Action<P> CreateSetter<P>(string name)
            => Reflection.CreateSetter<P>(this, name);
    }
}
