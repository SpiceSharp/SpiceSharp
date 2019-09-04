using SpiceSharp.Circuits;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Template for a behavior.
    /// </summary>
    public abstract class Behavior : IBehavior
    {
        /// <summary>
        /// Gets the parameters of the behavior.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public ParameterSetDictionary Parameters { get; } = new ParameterSetDictionary();

        /// <summary>
        /// Gets the identifier of the behavior.
        /// </summary>
        /// <remarks>
        /// This should be the same identifier as the entity that created it.
        /// </remarks>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Behavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected Behavior(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public virtual void Bind(BindingContext context)
        {
        }

        /// <summary>
        /// Destroy the behavior.
        /// </summary>
        public virtual void Unbind()
        {
        }

        #region Implementation of IParameterSet
        
        /// <summary>
        /// Sets the value of the principal parameter.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if a principal parameter was set; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool TrySetPrincipalParameter<T>(T value)
        {
            if (ParameterHelper.TrySetPrincipalParameter(this, value))
                return true;
            return Parameters.TrySetPrincipalParameter(value);
        }

        /// <summary>
        /// Sets the value of the principal parameters.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The source object (can be used for chaining).
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public object SetPrincipalParameter<T>(T value)
        {
            if (!ParameterHelper.TrySetPrincipalParameter(this, value))
                Parameters.SetPrincipalParameter(value);
            return this;
        }

        /// <summary>
        /// Tries to get the value of the principal parameter.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if a principal parameter was set; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool TryGetPrincipalParameter<T>(out T value)
        {
            if (ParameterHelper.TryGetPrincipalParameter(this, out value))
                return true;
            return Parameters.TryGetPrincipalParameter(out value);
        }

        /// <summary>
        /// Gets the value of the principal parameter.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>
        /// The value of the principal parameter.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public T GetPrincipalParameter<T>()
        {
            if (ParameterHelper.TryGetPrincipalParameter<T>(this, out var result))
                return result;
            return Parameters.GetPrincipalParameter<T>();
        }

        /// <summary>
        /// Creates a setter for the principal parameter.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <returns>
        /// An action that can set the value of the principal parameter, or <c>null</c> if there is no principal parameter.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public Action<T> CreatePrincipalSetter<T>()
        {
            var setter = ParameterHelper.CreatePrincipalSetter<T>(this);
            if (setter == null)
                setter = Parameters.CreatePrincipalSetter<T>();
            return setter;
        }

        /// <summary>
        /// Creates a getter for the principal parameter.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <returns>
        /// A function returning the value of the principal parameter, or <c>null</c> if there is no principal parameter.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public Func<T> CreatePrincipalGetter<T>()
        {
            var getter = ParameterHelper.CreatePrincipalGetter<T>(this);
            if (getter == null)
                getter = Parameters.CreatePrincipalGetter<T>();
            return getter;
        }

        /// <summary>
        /// Tries setting a parameter with a specified name.
        /// If multiple parameters have the same name, they will all be set.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}" />.</param>
        /// <returns>
        ///   <c>true</c> if there was one or more parameters set; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool TrySetParameter<T>(string name, T value, IEqualityComparer<string> comparer = null)
        {
            var result = ParameterHelper.TrySetParameter(this, name, value, comparer) ||
                Parameters.TrySetParameter(name, value, comparer);
            return result;
        }

        /// <summary>
        /// Sets a parameter with a specified name. If multiple parameters have the same name, they will all be set.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}" />.</param>
        /// <returns>
        /// The source object (can be used for chaining).
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public object SetParameter<T>(string name, T value, IEqualityComparer<string> comparer = null)
        {
            if (ParameterHelper.TrySetParameter(this, name, value, comparer))
                Parameters.SetParameter(name, value, comparer);
            else
                Parameters.TrySetParameter(name, value, comparer);
            return this;
        }

        /// <summary>
        /// Tries getting a parameter value. Only the first found parameter with the specified name is returned.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}" />.</param>
        /// <returns>
        ///   <c>true</c> if the parameter exists and the value was read; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool TryGetParameter<T>(string name, out T value, IEqualityComparer<string> comparer = null)
        {
            if (ParameterHelper.TryGetParameter(this, name, out value, comparer))
                return true;
            return Parameters.TryGetParameter(name, out value, comparer);
        }

        /// <summary>
        /// Gets a parameter value. Only the first found parameter with the specified name is returned.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}" />.</param>
        /// <returns>
        /// The parameter value.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public T GetParameter<T>(string name, IEqualityComparer<string> comparer = null)
        {
            if (ParameterHelper.TryGetParameter(this, name, out T value, comparer))
                return value;
            return Parameters.GetParameter<T>(name);
        }

        /// <summary>
        /// Returns a setter for the first eligible parameter with the specified name.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}" />.</param>
        /// <returns>
        /// A function returning the value of the parameter, or <c>null</c> if there is no parameter with the specified name.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public Action<T> CreateSetter<T>(string name, IEqualityComparer<string> comparer = null)
        {
            var setter = ParameterHelper.CreateSetter<T>(this, name, comparer);
            if (setter == null)
                setter = Parameters.CreateSetter<T>(name, comparer);
            return setter;
        }

        /// <summary>
        /// Returns a getter for the first found parameter with the specified name.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The string comparer used for identifying the parameter name.</param>
        /// <returns>
        /// A function returning the value of the parameter, or <c>null</c> if there is no parameter with the specified name.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public Func<T> CreateGetter<T>(string name, IEqualityComparer<string> comparer = null)
        {
            var getter = ParameterHelper.CreateGetter<T>(this, name, comparer);
            if (getter == null)
                getter = Parameters.CreateGetter<T>(name, comparer);
            return getter;
        }

        /// <summary>
        /// Tries to call a method by name without arguments.
        /// If multiple parameters by this name exist, all of them will be called.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}" />.</param>
        /// <returns>
        ///   <c>true</c> if there was one or more methods called; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool TrySetParameter(string name, IEqualityComparer<string> comparer = null)
        {
            return ParameterHelper.TrySetParameter(this, name, comparer) ||
                Parameters.TrySetParameter(name, comparer);
        }

        /// <summary>
        /// Calls a method by name without arguments.
        /// If multiple parameters by this name exist, all of them will be called.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}" />.</param>
        /// <returns>
        /// The source object (can be used for chaining).
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public object SetParameter(string name, IEqualityComparer<string> comparer = null)
        {
            if (!ParameterHelper.TrySetParameter(this, name, comparer))
                Parameters.SetParameter(name, comparer);
            else
                Parameters.TrySetParameter(name, comparer);
            return this;
        }
        #endregion
    }
}
