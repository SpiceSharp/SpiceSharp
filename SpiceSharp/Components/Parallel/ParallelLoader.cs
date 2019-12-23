using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A component that can execute multiple <see cref="IComponent"/> instances in parallel.
    /// </summary>
    /// <seealso cref="Entity" />
    /// <seealso cref="IComponent" />
    public class ParallelLoader : Entity, IComponent
    {
        private readonly IComponent[] _components;

        /// <summary>
        /// Gets or sets the model of the component.
        /// </summary>
        /// <value>
        /// The model of the component.
        /// </value>
        public string Model { get; set; }

        /// <summary>
        /// Gets the number of nodes.
        /// </summary>
        /// <value>
        /// The number of nodes.
        /// </value>
        public int PinCount { get; private set; }

        /// <summary>
        /// Connects the component in the circuit.
        /// </summary>
        /// <param name="nodes">The node indices.</param>
        /// <returns>
        /// The instance calling the method for chaining.
        /// </returns>
        public IComponent Connect(params string[] nodes)
        {
            // We don't really have any connections of our own
            return this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelLoader"/> class.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <param name="components">The components that can be executed in parallel.</param>
        public ParallelLoader(string name, IEnumerable<IComponent> components)
            : base(name)
        {
            PinCount = 0;
            foreach (var component in components)
                PinCount += component.PinCount;
            _components = components.ToArray();
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
        }

        /// <summary>
        /// Gets the node index of a pin.
        /// </summary>
        /// <param name="index">The pin index.</param>
        /// <returns>
        /// The node index.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
        public string GetNode(int index)
        {
            if (index < 0 || index >= PinCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            foreach (var component in _components)
            {
                if (index > component.PinCount)
                    index -= component.PinCount;
                else
                    return component.GetNode(index);
            }

            // This code should be unreachable
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        /// Gets the node indexes (in order).
        /// </summary>
        /// <param name="variables">The set of variables.</param>
        /// <returns>
        /// An enumerable for all nodes.
        /// </returns>
        public IEnumerable<Variable> MapNodes(IVariableSet variables)
        {
            variables.ThrowIfNull(nameof(variables));
            foreach (var component in _components)
            {
                foreach (var variable in component.MapNodes(variables))
                    yield return variable;
            }
        }
    }
}
