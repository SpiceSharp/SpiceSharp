using System;
using System.Collections.Generic;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A dictionary of <see cref="Behavior" />. Only on instance of each type is allowed.
    /// </summary>
    /// <seealso cref="TypeDictionary{Behavior}" />
    public class EntityBehaviors : TypeDictionary<IBehavior>, IParameterSet
    {
        /// <summary>
        /// Gets the source identifier.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets the parameters used by the behaviors.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public ParameterSetDictionary Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBehaviors"/> class.
        /// </summary>
        /// <param name="source">The entity identifier that will provide the behaviors.</param>
        public EntityBehaviors(string source)
        {
            Source = source.ThrowIfNull(nameof(source));
            Parameters = new ParameterSetDictionary();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBehaviors"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="parameters">The parameters.</param>
        public EntityBehaviors(string source, ParameterSetDictionary parameters)
        {
            Source = source.ThrowIfNull(nameof(source));
            Parameters = parameters.ThrowIfNull(nameof(parameters));
        }

        #region Implementation of IParameterSet   
        
        /*
         * In general, we first apply the set/get to the behaviors, then to the parameters.
         */

        /// <summary>
        /// Sets the value of the principal parameter.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if a principal parameter was set; otherwise <c>false</c>.
        /// </returns>
        public bool TrySetPrincipalParameter<T>(T value)
        {
            foreach (var b in Values)
            {
                if (b.TrySetPrincipalParameter(value))
                    return true;
            }
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
        public EntityBehaviors SetPrincipalParameter<T>(T value)
        {
            foreach (var b in Values)
            {
                if (b.TrySetPrincipalParameter(value))
                    return this;
            }
            Parameters.SetPrincipalParameter(value);
            return this;
        }
        object IParameterSet.SetPrincipalParameter<T>(T value) => SetPrincipalParameter(value);

        /// <summary>
        /// Tries to get the value of the principal parameter.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if a principal parameter was set; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetPrincipalParameter<T>(out T value)
        {
            foreach (var b in Values)
            {
                if (b.TryGetPrincipalParameter(out value))
                    return true;
            }
            return Parameters.TryGetPrincipalParameter(out value);
        }

        /// <summary>
        /// Gets the value of the principal parameter.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>
        /// The value of the principal parameter.
        /// </returns>
        public T GetPrincipalParameter<T>()
        {
            foreach (var b in Values)
            {
                if (b.TryGetPrincipalParameter(out T value))
                    return value;
            }
            return Parameters.GetPrincipalParameter<T>();
        }

        /// <summary>
        /// Creates a setter for the principal parameter.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <returns>
        /// An action that can set the value of the principal parameter, or <c>null</c> if there is no principal parameter.
        /// </returns>
        public Action<T> CreatePrincipalSetter<T>()
        {
            foreach (var b in Values)
            {
                var setter = b.CreatePrincipalSetter<T>();
                if (setter != null)
                    return setter;
            }
            return Parameters.CreatePrincipalSetter<T>();
        }

        /// <summary>
        /// Creates a getter for the principal parameter.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <returns>
        /// A function returning the value of the principal parameter, or <c>null</c> if there is no principal parameter.
        /// </returns>
        public Func<T> CreatePrincipalGetter<T>()
        {
            foreach (var b in Values)
            {
                var getter = b.CreatePrincipalGetter<T>();
                if (getter != null)
                    return getter;
            }
            return Parameters.CreatePrincipalGetter<T>();
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
        public bool TrySetParameter<T>(string name, T value, IEqualityComparer<string> comparer = null)
        {
            var result = false;
            foreach (var b in Values)
                result |= b.TrySetParameter(name, value, comparer);
            result |= Parameters.TrySetParameter(name, value, comparer);
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
        public EntityBehaviors SetParameter<T>(string name, T value, IEqualityComparer<string> comparer = null)
        {
            if (TrySetParameter(name, value, comparer))
                return this;
            throw new CircuitException("Could not find parameter '{0}'".FormatString(name));
        }
        object IParameterSet.SetParameter<T>(string name, T value, IEqualityComparer<string> comparer)
            => SetParameter(name, value, comparer);

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
        public bool TryGetParameter<T>(string name, out T value, IEqualityComparer<string> comparer = null)
        {
            foreach (var b in Values)
            {
                if (b.TryGetParameter(name, out value, comparer))
                    return true;
            }
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
        public T GetParameter<T>(string name, IEqualityComparer<string> comparer = null)
        {
            foreach (var b in Values)
            {
                if (b.TryGetParameter(name, out T value, comparer))
                    return value;
            }
            return Parameters.GetParameter<T>(name, comparer);
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
        public Action<T> CreateSetter<T>(string name, IEqualityComparer<string> comparer = null)
        {
            foreach (var b in Values)
            {
                var setter = b.CreateSetter<T>(name, comparer);
                if (setter != null)
                    return setter;
            }
            return Parameters.CreateSetter<T>(name, comparer);
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
        public Func<T> CreateGetter<T>(string name, IEqualityComparer<string> comparer = null)
        {
            foreach (var b in Values)
            {
                var getter = b.CreateGetter<T>(name, comparer);
                if (getter != null)
                    return getter;
            }
            return Parameters.CreateGetter<T>(name, comparer);
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
        public bool TrySetParameter(string name, IEqualityComparer<string> comparer = null)
        {
            var result = false;
            foreach (var b in Values)
                result |= b.TrySetParameter(name, comparer);
            result |= Parameters.TrySetParameter(name, comparer);
            return result;
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
        public EntityBehaviors SetParameter(string name, IEqualityComparer<string> comparer = null)
        {
            if (TrySetParameter(name, comparer))
                return this;
            throw new CircuitException("Could not find parameter '{0}'".FormatString(name));
        }
        object IParameterSet.SetParameter(string name, IEqualityComparer<string> comparer)
            => SetParameter(name, comparer);
        #endregion
    }
}
