using System;

namespace SpiceSharp
{
    /// <summary>
    /// A dictionary of <see cref="ParameterSet" />. Only one instance of each type is allowed.
    /// </summary>
    /// <seealso cref="TypeDictionary{P}" />
    public class ParameterSetDictionary : TypeDictionary<IParameterSet>, IParameterSetDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSetDictionary"/> class.
        /// </summary>
        public ParameterSetDictionary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSetDictionary"/> class.
        /// </summary>
        /// <param name="hierarchy">if set to <c>true</c>, the dictionary can be searched by parent classes and interfaces.</param>
        public ParameterSetDictionary(bool hierarchy)
            : base(hierarchy)
        {
        }

        /// <summary>
        /// Clones the dictionary.
        /// </summary>
        /// <returns></returns>
        public virtual IParameterSetDictionary Clone()
        {
            var clone = (ParameterSetDictionary)Activator.CreateInstance(GetType(), StoreHierarchy);
            foreach (var p in Dictionary)
                clone.Dictionary.Add(p.Key, (IParameterSet)p.Value.Clone());
            return clone;
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns></returns>
        ICloneable ICloneable.Clone() => Clone();

        /// <summary>
        /// Copy all parameter sets.
        /// </summary>
        /// <param name="source">The source object.</param>
        public virtual void CopyFrom(IParameterSetDictionary source)
        {
            var d = source as ParameterSetDictionary ?? throw new CircuitException("Cannot copy, type mismatch");
            foreach (var ps in d.Values)
            {
                // If the parameter set doesn't exist, then we will simply clone it, else copy them
                if (!TryGetValue(ps.GetType(), out var myps))
                    Add((IParameterSet)ps.Clone());
                else
                    Reflection.CopyPropertiesAndFields(ps, myps);
            }
        }

        /// <summary>
        /// Copy all properties from another object to this one.
        /// </summary>
        /// <param name="source">The source object.</param>
        void ICloneable.CopyFrom(ICloneable source) => CopyFrom((ParameterSetDictionary)source);

        /// <summary>
        /// Call a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <returns>The current instance for chaining.</returns>
        public IParameterSetDictionary Set(string name)
        {
            foreach (var ps in Dictionary.Values)
            {
                if (ps.TrySet(name))
                    return this;
            }
            throw new ParameterNotFoundException(name, this);
        }

        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current instance for chaining.</returns>
        public IParameterSetDictionary Set<P>(string name, P value)
        {
            foreach (var ps in Dictionary.Values)
            {
                if (ps.TrySet(name, value))
                    return this;
            }
            throw new ParameterNotFoundException(name, this);
        }

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public void CalculateDefaults()
        {
            foreach (var ps in Dictionary.Values)
                ps.CalculateDefaults();
        }

        /// <summary>
        /// Call a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        void IImportParameterSet.Set(string name)
        {
            foreach (var ps in Dictionary.Values)
            {
                if (ps.TrySet(name))
                    return;
            }
            throw new ParameterNotFoundException(name, this);
        }

        /// <summary>
        /// Tries calling a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// <c>true</c> if the method was called; otherwise <c>false</c>.
        /// </returns>
        public bool TrySet(string name)
        {
            foreach (var ps in Dictionary.Values)
            {
                if (ps.TrySet(name))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        void IImportParameterSet.Set<P>(string name, P value)
        {
            foreach (var ps in Dictionary.Values)
            {
                if (ps.TrySet(name, value))
                    return;
            }
            throw new ParameterNotFoundException(name, this);
        }

        /// <summary>
        /// Tries to set the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the parameter was set; otherwise <c>false</c>.
        /// </returns>
        public bool TrySet<P>(string name, P value)
        {
            foreach (var ps in Dictionary.Values)
            {
                if (ps.TrySet(name, value))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns>The value.</returns>
        public P Get<P>(string name)
        {
            foreach (var ps in Dictionary.Values)
            {
                if (ps.TryGet(name, out P result))
                    return result;
            }
            throw new ParameterNotFoundException(name, this);
        }

        /// <summary>
        /// Tries to get the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the parameter was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGet<P>(string name, out P value)
        {
            foreach (var ps in Dictionary.Values)
            {
                if (ps.TryGet(name, out value))
                    return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Creates a getter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A getter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        public Func<P> CreateGetter<P>(string name)
        {
            foreach (var ps in Dictionary.Values)
            {
                var result = ps.CreateGetter<P>(name);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Creates a setter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A setter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        public Action<P> CreateSetter<P>(string name)
        {
            foreach (var ps in Dictionary.Values)
            {
                var result = ps.CreateSetter<P>(name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
