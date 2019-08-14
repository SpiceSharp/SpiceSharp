using System.Collections.Generic;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Instance data for using in combination with components.
    /// </summary>
    public class ComponentInstanceData : InstanceData
    {
        /// <summary>
        /// A map for nodes when instancing.
        /// </summary>
        public Dictionary<string, string> NodeMap { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ComponentInstanceData"/> class.
        /// </summary>
        /// <param name="subcircuit">The circuit to instantiate.</param>
        public ComponentInstanceData(Circuit subcircuit)
            : this(subcircuit, null, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ComponentInstanceData"/> class.
        /// </summary>
        /// <param name="subckt">The circuit to instantiate.</param>
        /// <param name="name">The instance name.</param>
        public ComponentInstanceData(Circuit subckt, string name)
            : this(subckt, name, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ComponentInstanceData"/> class.
        /// </summary>
        /// <param name="subckt">The circuit to instantiate.</param>
        /// <param name="name">The instance name.</param>
        /// <param name="comparer">The <seealso cref="IEqualityComparer{T}"/> class to compare node names.</param>
        public ComponentInstanceData(Circuit subckt, string name, IEqualityComparer<string> comparer)
            : base(subckt, name)
        {
            NodeMap = new Dictionary<string, string>(comparer ?? EqualityComparer<string>.Default);
        }

        /// <summary>
        /// Generate a node name. If <seealso cref="NodeMap"/> contains the node, it is mapped, else
        /// the node name is expanded.
        /// </summary>
        /// <param name="name">the local name.</param>
        /// <returns></returns>
        public virtual string GenerateNodeName(string name)
        {
            if (NodeMap.TryGetValue(name, out var newname))
                return newname;
            return Utility.Combine(Name, name);
        }

        /// <summary>
        /// Generate a model name. If the model name exists in the subcircuit, the name is expanded.
        /// </summary>
        /// <param name="name">The local name.</param>
        /// <returns></returns>
        public virtual string GenerateModelName(string name)
        {
            if (Subcircuit.Contains(name))
                return Utility.Combine(Name, name);
            return name;
        }
    }
}
