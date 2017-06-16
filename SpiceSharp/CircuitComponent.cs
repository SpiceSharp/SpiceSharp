using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Parameters;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;
using SpiceSharp.Circuits;

namespace SpiceSharp
{
    /// <summary>
    /// This class represent a component
    /// </summary>
    public abstract class CircuitComponent : Parameterized
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private string[] terminals;

        /// <summary>
        /// This parameter can change the order in which components are traversed
        /// Components with a higher priority will get the first chance to execute their methods
        /// </summary>
        public int Priority { get; protected set; } = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the component</param>
        public CircuitComponent(string name, int pins) : base(name)
        {
            terminals = new string[pins];
        }

        /// <summary>
        /// Connect the component in the circuit
        /// </summary>
        /// <param name="nodes"></param>
        public void Connect(params string[] nodes)
        {
            if (nodes.Length != terminals.Length)
                throw new CircuitException($"{Name}: Node count mismatch. {nodes.Length} given, {terminals.Length} expected.");
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] == null)
                    throw new ArgumentNullException("node " + (i + 1));
                terminals[i] = nodes[i];
            }
        }

        /// <summary>
        /// Get the model for this component
        /// </summary>
        /// <returns>Returns null if no model is available</returns>
        public abstract CircuitModel GetModel();

        /// <summary>
        /// Helper function for binding nodes to the circuit
        /// </summary>
        /// <param name="ckt"></param>
        /// <param name="extra"></param>
        /// <returns></returns>
        protected CircuitNode[] BindNodes(Circuit ckt, params CircuitNode.NodeType[] extra)
        {
            // Map connected nodes
            CircuitNode[] nodes = new CircuitNode[terminals.Length + extra.Length];
            for (int i = 0; i < terminals.Length; i++)
                nodes[i] = ckt.Nodes.Map(terminals[i]);

            // Map extra equations
            for (int i = 0; i < extra.Length; i++)
                nodes[i + terminals.Length] = ckt.Nodes.Map(null, extra[i]);

            // Return all nodes
            return nodes;
        }

        /// <summary>
        /// Setup the component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public abstract void Setup(Circuit ckt);

        /// <summary>
        /// Use initial conditions for the device
        /// </summary>
        /// <param name="ckt"></param>
        public virtual void SetIc(Circuit ckt)
        {
            // Do nothing
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt"></param>
        public abstract void Temperature(Circuit ckt);

        /// <summary>
        /// Load the component in the current circuit state
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public abstract void Load(Circuit ckt);

        /// <summary>
        /// Load the component in the current circuit state for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public virtual void AcLoad(Circuit ckt)
        {
            // Do nothing
        }

        /// <summary>
        /// Accept the current timepoint as the solution
        /// </summary>
        /// <param name="ckt"></param>
        public virtual void Accept(Circuit ckt)
        {
            // Do nothing
        }

        /// <summary>
        /// Unsetup/destroy the component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public virtual void Unsetup(Circuit ckt)
        {
            // Do nothing
        }

        /// <summary>
        /// Check convergence for this component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <returns>True if converges</returns>
        public virtual bool IsConvergent(Circuit ckt)
        {
            return true;
        }
    }
}
