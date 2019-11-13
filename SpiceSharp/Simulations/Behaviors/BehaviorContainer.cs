using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A dictionary of <see cref="Behavior" />. Only on instance of each type is allowed.
    /// </summary>
    /// <seealso cref="TypeDictionary{Behavior}" />
    public class BehaviorContainer : TypeDictionary<IBehavior>, IBehaviorContainer
    {
        /// <summary>
        /// Gets the source identifier.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the parameters used by the behaviors.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public ParameterSetDictionary Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainer"/> class.
        /// </summary>
        /// <param name="source">The entity identifier that will provide the behaviors.</param>
        public BehaviorContainer(string source)
            : base(true)
        {
            Name = source.ThrowIfNull(nameof(source));
            Parameters = new ParameterSetDictionary();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainer"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="parameters">The parameters.</param>
        public BehaviorContainer(string source, ParameterSetDictionary parameters)
            : base(true)
        {
            Name = source.ThrowIfNull(nameof(source));
            Parameters = parameters.ThrowIfNull(nameof(parameters));
        }

        #region Implementation of IParameterSet   

        /*
         * In general, we first apply the set/get to the behaviors, then to the parameters.
         */

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
            foreach (var b in Values)
            {
                if (b.TrySetParameter(value))
                    return true;
            }
            return Parameters.TrySetParameter(value);
        }

        /// <summary>
        /// Sets the value of the principal parameters.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The source object (can be used for chaining).
        /// </returns>
        public IBehaviorContainer SetParameter<P>(P value)
        {
            foreach (var b in Values)
            {
                if (b.TrySetParameter(value))
                    return this;
            }
            Parameters.SetParameter(value);
            return this;
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
            foreach (var b in Values)
            {
                if (b.TryGetParameter(out value))
                    return true;
            }
            return Parameters.TryGetParameter(out value);
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
            foreach (var b in Values)
            {
                if (b.TryGetParameter(out P value))
                    return value;
            }
            return Parameters.GetParameter<P>();
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
            foreach (var b in Values)
            {
                var setter = b.CreateSetter<P>();
                if (setter != null)
                    return setter;
            }
            return Parameters.CreateSetter<P>();
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
            foreach (var b in Values)
            {
                var getter = b.CreateGetter<P>();
                if (getter != null)
                    return getter;
            }
            return Parameters.CreateGetter<P>();
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
            var result = false;
            foreach (var b in Values)
                result |= b.TrySetParameter(name, value);
            result |= Parameters.TrySetParameter(name, value);
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
        public IBehaviorContainer SetParameter<P>(string name, P value)
        {
            if (TrySetParameter(name, value))
                return this;
            throw new CircuitException("Could not find parameter '{0}'".FormatString(name));
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
            foreach (var b in Values)
            {
                if (b.TryGetParameter(name, out value))
                    return true;
            }
            return Parameters.TryGetParameter(name, out value);
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
            foreach (var b in Values)
            {
                if (b.TryGetParameter(name, out P value))
                    return value;
            }
            return Parameters.GetParameter<P>(name);
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
            foreach (var b in Values)
            {
                var setter = b.CreateSetter<P>(name);
                if (setter != null)
                    return setter;
            }
            return Parameters.CreateSetter<P>(name);
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
            foreach (var b in Values)
            {
                var getter = b.CreateGetter<P>(name);
                if (getter != null)
                    return getter;
            }
            return Parameters.CreateGetter<P>(name);
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
            foreach (var b in Values)
                result |= b.TryCall(name);
            result |= Parameters.TryCall(name);
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
        public IBehaviorContainer Call(string name)
        {
            if (TrySetParameter(name))
                return this;
            throw new CircuitException("Could not find parameter '{0}'".FormatString(name));
        }
        #endregion
    }
}
