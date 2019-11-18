using System;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// A dictionary of <see cref="ParameterSet" />. Only one instance of each type is allowed.
    /// </summary>
    /// <seealso cref="TypeDictionary{P}" />
    public class ParameterSetDictionary : TypeDictionary<IParameterSet>, IParameterSetDictionary
    {
        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        IEnumerable<INamedParameters> INamedParameterCollection.NamedParameters => Values;

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
    }
}
