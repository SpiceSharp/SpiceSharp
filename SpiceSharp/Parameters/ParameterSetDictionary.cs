using System;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// A dictionary of <see cref="ParameterSet" />. Only one instance of each type is allowed.
    /// </summary>
    /// <seealso cref="TypeDictionary{T}" />
    public class ParameterSetDictionary : TypeDictionary<ParameterSet>, ICloneable, ICloneable<ParameterSetDictionary>, IParameterSet
    {
        /// <summary>
        /// Adds a parameter set to the dictionary.
        /// </summary>
        /// <param name="set">The parameter set.</param>
        public void Add(ParameterSet set)
        {
            set.ThrowIfNull(nameof(set));
            Add(set.GetType(), set);
        }

        /// <summary>
        /// Clone the dictionary.
        /// </summary>
        /// <returns></returns>
        public virtual ParameterSetDictionary Clone()
        {
            var clone = Activator.CreateInstance(GetType());
            Reflection.CopyPropertiesAndFields(this, clone);
            return (ParameterSetDictionary)clone;
        }

        /// <summary>
        /// Clone the object.
        /// </summary>
        /// <returns></returns>
        ICloneable ICloneable.Clone() => Clone();

        /// <summary>
        /// Copy all parameter sets.
        /// </summary>
        /// <param name="source">The source object.</param>
        public virtual void CopyFrom(ParameterSetDictionary source)
        {
            var d = source as ParameterSetDictionary ?? throw new CircuitException("Cannot copy, type mismatch");
            foreach (var ps in d.Values)
            {
                // If the parameter set doesn't exist, then we will simply clone it, else copy them
                if (!TryGetValue(ps.GetType(), out var myps))
                    Add(ps.Clone());
                else
                    Reflection.CopyPropertiesAndFields(ps, myps);
            }
        }

        /// <summary>
        /// Copy all properties from another object to this one.
        /// </summary>
        /// <param name="source">The source object.</param>
        void ICloneable.CopyFrom(ICloneable source) => CopyFrom((ParameterSetDictionary)source);

        #region Implement the IParameterSet interface        
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
            foreach (var p in Values)
            {
                if (p.TrySetPrincipalParameter(value))
                    return true;
            }
            return false;
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
        public ParameterSetDictionary SetPrincipalParameter<T>(T value)
        {
            foreach (var p in Values)
            {
                if (p.TrySetPrincipalParameter(value))
                    return this;
            }
            throw new CircuitException("No principal parameter found");
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
        /// <exception cref="NotImplementedException"></exception>
        public bool TryGetPrincipalParameter<T>(out T value)
        {
            foreach (var p in Values)
            {
                if (p.TryGetPrincipalParameter(out value))
                    return true;
            }
            value = default;
            return false;
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
            foreach (var p in Values)
            {
                if (p.TryGetPrincipalParameter(out T value))
                    return value;
            }
            throw new CircuitException("No principal parameter found");
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
            foreach (var p in Values)
            {
                var setter = p.CreatePrincipalSetter<T>();
                if (setter != null)
                    return setter;
            }
            return null;
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
            foreach (var p in Values)
            {
                var getter = p.CreatePrincipalGetter<T>();
                if (getter != null)
                    return getter;
            }
            return null;
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
            bool result = false;
            foreach (var p in Values)
                result |= p.TrySetParameter(name, value, comparer);
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
        public ParameterSetDictionary SetParameter<T>(string name, T value, IEqualityComparer<string> comparer = null)
        {
            if (TrySetParameter(name, value, comparer))
                return this;
            throw new CircuitException("Could not find a parameter '{0}'".FormatString(name));
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
        /// <exception cref="NotImplementedException"></exception>
        public bool TryGetParameter<T>(string name, out T value, IEqualityComparer<string> comparer = null)
        {
            foreach (var p in Values)
            {
                if (p.TryGetParameter(name, out value, comparer))
                    return true;
            }
            value = default;
            return false;
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
            if (TryGetParameter(name, out T value, comparer))
                return value;
            throw new CircuitException("Could not find parameter '{0}'".FormatString(name));
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
            foreach (var p in Values)
            {
                var setter = p.CreateSetter<T>(name, comparer);
                if (setter != null)
                    return setter;
            }
            return null;
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
            foreach (var p in Values)
            {
                var getter = p.CreateGetter<T>(name, comparer);
                if (getter != null)
                    return getter;
            }
            return null;
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
            foreach (var p in Values)
                result |= p.TrySetParameter(name, comparer);
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
        public ParameterSetDictionary SetParameter(string name, IEqualityComparer<string> comparer = null)
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
