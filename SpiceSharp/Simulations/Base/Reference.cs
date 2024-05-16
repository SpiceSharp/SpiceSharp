using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Common;
using SpiceSharp.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Simulations.Base
{
    /// <summary>
    /// A reference to a hierarchical item.
    /// </summary>
    /// <seealso cref="IEnumerable{T}"/>
    /// <seealso cref="IEquatable{T}"/>
    public readonly struct Reference : IEnumerable<string>, IEquatable<Reference>
    {
        /// <summary>
        /// Gets the path of the node.
        /// </summary>
        public IReadOnlyList<string> Path { get; }

        /// <summary>
        /// Gets the length of the path.
        /// </summary>
        public int Length => Path?.Count ?? 0;

        /// <summary>
        /// Gets an element of the hierarchical reference at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Returns the path item at the specified index.</returns>
        public string this[int index] => Path[index];

        /// <summary>
        /// Creates a new <see cref="Reference"/>.
        /// </summary>
        /// <param name="node">The node name.</param>
        public Reference(string node)
        {
            Path = [node];
        }

        /// <summary>
        /// Creates a new <see cref="Reference"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        public Reference(params string[] path)
        {
            Path = path ?? [];
        }

        /// <summary>
        /// Calculates a hash code for the node reference.
        /// </summary>
        /// <returns></returns>
        public override readonly int GetHashCode()
        {
            int hash = 0;
            for (int i = 0; i < Path.Count; i++)
                hash = (hash * 1021) ^ (Path[i]?.GetHashCode() ?? 0);
            return hash;
        }

        /// <summary>
        /// Determines whether the object is equal to another.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>Returns <c>true</c> if both are equal; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj) => obj is Reference other && Equals(other);

        /// <summary>
        /// Determines whether the node reference is equal to another.
        /// </summary>
        /// <param name="other">The other node reference.</param>
        /// <returns>Returns <c>true</c> if both are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(Reference other)
        {
            if (Path.Count != other.Path.Count)
                return false;
            for (int i = 0; i < Path.Count; i++)
            {
                if (Path[i] is null && other.Path[i] is null)
                    continue;
                if (Path[i] is null || other.Path[i] is null)
                    return false;
                if (!Path[i].Equals(other.Path[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the behavior container that the hierarchical reference points to.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <returns>Returns the behavior container.</returns>
        /// <exception cref="BehaviorsNotFoundException">Thrown if a behavior collection could not be found.</exception>
        public IBehaviorContainer GetContainer(ISimulation simulation)
        {
            if (Length == 0)
                return null;
            if (!simulation.EntityBehaviors.TryGetBehaviors(Path[0], out var container))
                throw new BehaviorsNotFoundException(Path[0], Properties.Resources.Behaviors_NoBehaviorFor.FormatString(Path[0]));
            for (int i = 1; i < Path.Count; i++)
            {
                if (!container.TryGetValue<IEntitiesBehavior>(out var entitiesBehavior) ||
                    !entitiesBehavior.LocalBehaviors.TryGetBehaviors(Path[i], out container))
                    throw new BehaviorsNotFoundException(Path[i], Properties.Resources.Behaviors_NoBehaviorFor.FormatString(Path[i]));
            }
            return container;
        }

        /// <summary>
        /// Tries to find a behavior container that the hierarchical reference points to.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="container">The behavior container.</param>
        /// <returns>Returns <c>true</c> if the container could be found; otherwise, <c>false</c>.</returns>
        public bool TryGetContainer(ISimulation simulation, out IBehaviorContainer container)
        {
            container = null;
            if (Length == 0)
                return false;

            if (!simulation.EntityBehaviors.TryGetBehaviors(Path[0], out container))
                return false;
            for (int i = 1; i < Path.Count; i++)
            {
                if (!container.TryGetValue<IEntitiesBehavior>(out var entitiesBehavior) ||
                    !entitiesBehavior.LocalBehaviors.TryGetBehaviors(Path[i], out container))
                {
                    container = null;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the variable that the hierarchical reference points to.
        /// </summary>
        /// <typeparam name="T">The variable return type.</typeparam>
        /// <typeparam name="S">The simulation state that needs to be used to find the variable.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <returns>Returns the variable.</returns>
        /// <exception cref="VariableNotFoundException">Thrown if the variable could not be found.</exception>
        /// <exception cref="StateNotFoundException">Thrown if the simulation or entities behavior does not define the state.</exception>
        /// <exception cref="BehaviorsNotFoundException">Thrown if a behavior collection could not be found.</exception>
        public IVariable<T> GetVariable<T, S>(ISimulation simulation) where S : ISolverSimulationState<T>
        {
            if (Length == 0)
                return null;

            S state;
            int last = Path.Count - 1;
            if (Path.Count == 1)
            {
                // Search for the variable inside the simulation itself
                if (!simulation.TryGetState(out state))
                    throw new StateNotFoundException(typeof(S).Name, Properties.Resources.States_StateNotFoundFor.FormatString(typeof(S).Name));
            }
            else
            {
                if (!simulation.EntityBehaviors.TryGetBehaviors(Path[0], out var container))
                    throw new BehaviorsNotFoundException(Path[0], Properties.Resources.Behaviors_NoBehaviorFor.FormatString(Path[0]));
                for (int i = 1; i < last; i++)
                {
                    if (!container.TryGetValue<IEntitiesBehavior>(out var entitiesBehavior) ||
                        !entitiesBehavior.LocalBehaviors.TryGetBehaviors(Path[i], out container))
                        throw new BehaviorsNotFoundException(Path[i], Properties.Resources.Behaviors_NoBehaviorFor.FormatString(Path[i]));
                }
                if (!container.TryGetValue<IEntitiesBehavior>(out var eb))
                    throw new VariableNotFoundException(this, Properties.Resources.Variables_NoVariableFor.FormatString(ToString()));
                if (!eb.TryGetState(out state))
                    throw new StateNotFoundException(typeof(S).Name, Properties.Resources.States_StateNotFoundFor.FormatString(typeof(S).Name));
            }

            // Get the variable
            if (!state.TryGetValue(Path[Path.Count - 1], out var result))
                throw new VariableNotFoundException(this, Properties.Resources.Variables_NoVariableFor.FormatString(ToString()));
            return result;
        }

        /// <summary>
        /// Tries to find a variable that the hierarchical reference points to.
        /// </summary>
        /// <typeparam name="T">The variable return type.</typeparam>
        /// <typeparam name="S">The simulation state used to find the variable.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <param name="variable">The variable.</param>
        /// <returns>Returns <c>true</c> if the variable was found; otherwise, <c>false</c>.</returns>
        public bool TryGetVariable<T, S>(ISimulation simulation, out IVariable<T> variable) where S : ISolverSimulationState<T>
        {
            variable = default;
            if (Length == 0)
                return false;

            S state;
            int last = Length - 1;
            if (Length == 1)
            {
                if (!simulation.TryGetState(out state))
                    return false;
            }
            else
            {
                if (!simulation.EntityBehaviors.TryGetBehaviors(Path[0], out var container) ||
                    !container.TryGetValue<IEntitiesBehavior>(out var entitiesBehavior))
                    return false;
                for (int i = 1; i < last; i++)
                {
                    if (!entitiesBehavior.LocalBehaviors.TryGetBehaviors(Path[i], out container) ||
                        !container.TryGetValue(out entitiesBehavior))
                        return false;
                }
                if (!entitiesBehavior.TryGetState(out state))
                    return false;
            }

            if (state is null)
                return false;
            return state.TryGetValue(Path[last], out variable);
        }

        /// <summary>
        /// Gets the right-hand side vector element that relates to the node the hierarchical reference points to.
        /// </summary>
        /// <typeparam name="T">The variable return type.</typeparam>
        /// <typeparam name="S">The simulation state that needs to be used to find the variable.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <returns>Returns the variable.</returns>
        /// <exception cref="VariableNotFoundException">Thrown if the variable could not be found.</exception>
        /// <exception cref="StateNotFoundException">Thrown if the simulation or entities behavior does not define the state.</exception>
        /// <exception cref="BehaviorsNotFoundException">Thrown if a behavior collection could not be found.</exception>
        public Element<T> GetVectorElement<T, S>(ISimulation simulation) where S : ISolverSimulationState<T>
        {
            if (Length == 0)
                return null;

            S state;
            int last = Path.Count - 1;
            if (Path.Count == 1)
            {
                // Search for the variable inside the simulation itself
                if (!simulation.TryGetState(out state))
                    throw new StateNotFoundException(typeof(S).Name, Properties.Resources.States_StateNotFoundFor.FormatString(typeof(S).Name));
            }
            else
            {
                if (!simulation.EntityBehaviors.TryGetBehaviors(Path[0], out var container))
                    throw new BehaviorsNotFoundException(Path[0], Properties.Resources.Behaviors_NoBehaviorFor.FormatString(Path[0]));
                for (int i = 1; i < last; i++)
                {
                    if (!container.TryGetValue<IEntitiesBehavior>(out var entitiesBehavior) ||
                        !entitiesBehavior.LocalBehaviors.TryGetBehaviors(Path[i], out container))
                        throw new BehaviorsNotFoundException(Path[i], Properties.Resources.Behaviors_NoBehaviorFor.FormatString(Path[i]));
                }
                if (!container.TryGetValue<IEntitiesBehavior>(out var eb))
                    throw new VariableNotFoundException(this, Properties.Resources.Variables_NoVariableFor.FormatString(ToString()));
                if (!eb.TryGetState(out state))
                    throw new StateNotFoundException(typeof(S).Name, Properties.Resources.States_StateNotFoundFor.FormatString(typeof(S).Name));
            }

            // Get the variable
            if (!state.ContainsKey(Path[last]))
                throw new VariableNotFoundException(this, Properties.Resources.Variables_NoVariableFor.FormatString(ToString()));
            return state.Solver.GetElement(state.Map[state.GetSharedVariable(Path[last])]);
        }

        /// <summary>
        /// Tries to find a right-hand side vector element that relates to the node the hierarchical reference points to.
        /// </summary>
        /// <typeparam name="T">The variable return type.</typeparam>
        /// <typeparam name="S">The simulation state used to find the variable.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <param name="variable">The variable.</param>
        /// <returns>Returns <c>true</c> if the variable was found; otherwise, <c>false</c>.</returns>
        public bool TryGetVectorElement<T, S>(ISimulation simulation, out Element<T> variable) where S : ISolverSimulationState<T>
        {
            variable = default;
            if (Length == 0)
                return false;

            S state;
            int last = Length - 1;
            if (Length == 1)
            {
                if (!simulation.TryGetState(out state))
                    return false;
            }
            else
            {
                if (!simulation.EntityBehaviors.TryGetBehaviors(Path[0], out var container) ||
                    !container.TryGetValue<IEntitiesBehavior>(out var entitiesBehavior))
                    return false;
                for (int i = 1; i < last; i++)
                {
                    if (!entitiesBehavior.LocalBehaviors.TryGetBehaviors(Path[i], out container) ||
                        !container.TryGetValue(out entitiesBehavior))
                        return false;
                }
                if (!entitiesBehavior.TryGetState(out state))
                    return false;
            }

            // Get the variable
            if (state is null)
                return false;
            if (!state.ContainsKey(Path[last]))
                return false;
            variable = state.Solver.GetElement(state.Map[state.GetSharedVariable(Path[last])]);
            return true;
        }

        /// <summary>
        /// Converts the node reference to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
            => string.Join(Utility.Separator, Path);

        /// <summary>
        /// Gets an enumerator for the node reference.
        /// </summary>
        /// <returns>The enumerator.</returns>
        IEnumerator<string> IEnumerable<string>.GetEnumerator() => Path.GetEnumerator();

        /// <summary>
        /// Gets an enumerator for the node reference.
        /// </summary>
        /// <returns>The enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Path).GetEnumerator();

        /// <summary>
        /// Implicitly converts a string to a node reference.
        /// </summary>
        /// <param name="node">The node name.</param>
        public static implicit operator Reference(string node)
            => new(node);

        /// <summary>
        /// Implicitly converts an array of strings to a node reference.
        /// </summary>
        /// <param name="path">The path.</param>
        public static implicit operator Reference(string[] path)
            => new(path.ToArray());

        /// <summary>
        /// Implicitly converts a list of strings to a node reference.
        /// </summary>
        /// <param name="path">The path.</param>
        public static implicit operator Reference(List<string> path)
            => new(path.ToArray());
    }
}
