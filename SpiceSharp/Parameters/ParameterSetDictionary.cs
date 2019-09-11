using System;

namespace SpiceSharp
{
    /// <summary>
    /// A dictionary of <see cref="ParameterSet" />. Only one instance of each type is allowed.
    /// </summary>
    /// <seealso cref="TypeDictionary{P}" />
    public class ParameterSetDictionary : TypeDictionary<ParameterSet>, ICloneable, ICloneable<ParameterSetDictionary>, IParameterSet<ParameterSetDictionary>
    {
        /// <summary>
        /// Clone the dictionary.
        /// </summary>
        /// <returns></returns>
        public virtual ParameterSetDictionary Clone()
        {
            var clone = (ParameterSetDictionary)Activator.CreateInstance(GetType());
            foreach (var p in Values)
                clone.Add(p.Clone());
            return clone;
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
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if a principal parameter was set; otherwise <c>false</c>.
        /// </returns>
        public bool TrySetParameter<P>(P value)
        {
            foreach (var p in Values)
            {
                if (p.TrySetParameter(value))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the value of the principal parameters.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The source object (can be used for chaining).
        /// </returns>
        public ParameterSetDictionary SetParameter<P>(P value)
        {
            foreach (var p in Values)
            {
                if (p.TrySetParameter(value))
                    return this;
            }
            throw new CircuitException("No principal parameter found");
        }

        /// <summary>
        /// Tries to get the value of the principal parameter.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if a principal parameter was set; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetParameter<P>(out P value)
        {
            foreach (var p in Values)
            {
                if (p.TryGetParameter(out value))
                    return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Gets the value of the principal parameter.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <returns>
        /// The value of the principal parameter.
        /// </returns>
        public P GetParameter<P>()
        {
            foreach (var p in Values)
            {
                if (p.TryGetParameter(out P value))
                    return value;
            }
            throw new CircuitException("No principal parameter found");
        }

        /// <summary>
        /// Creates a setter for the principal parameter.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <returns>
        /// An action that can set the value of the principal parameter, or <c>null</c> if there is no principal parameter.
        /// </returns>
        public Action<P> CreateSetter<P>()
        {
            foreach (var p in Values)
            {
                var setter = p.CreateSetter<P>();
                if (setter != null)
                    return setter;
            }
            return null;
        }

        /// <summary>
        /// Creates a getter for the principal parameter.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <returns>
        /// A function returning the value of the principal parameter, or <c>null</c> if there is no principal parameter.
        /// </returns>
        public Func<P> CreateGetter<P>()
        {
            foreach (var p in Values)
            {
                var getter = p.CreateGetter<P>();
                if (getter != null)
                    return getter;
            }
            return null;
        }

        /// <summary>
        /// Tries setting a parameter with a specified name.
        /// If multiple parameters have the same name, they will all be set.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if there was one or more parameters set; otherwise <c>false</c>.
        /// </returns>
        public bool TrySetParameter<P>(string name, P value)
        {
            bool result = false;
            foreach (var p in Values)
                result |= p.TrySetParameter(name, value);
            return result;
        }

        /// <summary>
        /// Sets a parameter with a specified name. If multiple parameters have the same name, they will all be set.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The source object (can be used for chaining).
        /// </returns>
        public ParameterSetDictionary SetParameter<P>(string name, P value)
        {
            if (TrySetParameter(name, value))
                return this;
            throw new CircuitException("Could not find a parameter '{0}'".FormatString(name));
        }

        /// <summary>
        /// Tries getting a parameter value. Only the first found parameter with the specified name is returned.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the parameter exists and the value was read; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetParameter<P>(string name, out P value)
        {
            foreach (var p in Values)
            {
                if (p.TryGetParameter(name, out value))
                    return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Gets a parameter value. Only the first found parameter with the specified name is returned.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// The parameter value.
        /// </returns>
        public P GetParameter<P>(string name)
        {
            if (TryGetParameter(name, out P value))
                return value;
            throw new CircuitException("Could not find parameter '{0}'".FormatString(name));
        }

        /// <summary>
        /// Returns a setter for the first eligible parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A function returning the value of the parameter, or <c>null</c> if there is no parameter with the specified name.
        /// </returns>
        public Action<P> CreateSetter<P>(string name)
        {
            foreach (var p in Values)
            {
                var setter = p.CreateSetter<P>(name);
                if (setter != null)
                    return setter;
            }
            return null;
        }

        /// <summary>
        /// Returns a getter for the first found parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A function returning the value of the parameter, or <c>null</c> if there is no parameter with the specified name.
        /// </returns>
        public Func<P> CreateGetter<P>(string name)
        {
            foreach (var p in Values)
            {
                var getter = p.CreateGetter<P>(name);
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
        /// <returns>
        /// <c>true</c> if there was one or more methods called; otherwise <c>false</c>.
        /// </returns>
        public bool TryCall(string name)
        {
            var result = false;
            foreach (var p in Values)
                result |= p.TryCall(name);
            return result;
        }

        /// <summary>
        /// Calls a method by name without arguments.
        /// If multiple parameters by this name exist, all of them will be called.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <returns>
        /// The source object (can be used for chaining).
        /// </returns>
        public ParameterSetDictionary Call(string name)
        {
            if (TryCall(name))
                return this;
            throw new CircuitException("Could not find parameter '{0}'".FormatString(name));
        }
        #endregion
    }
}
