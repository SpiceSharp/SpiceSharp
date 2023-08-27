using SpiceSharp.Components.Subcircuits;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export real voltages.
    /// </summary>
    /// <seealso cref="Export{S, T}" />
    public class RealVoltageExport : Export<IBiasingSimulation, double>
    {
        /// <summary>
        /// Gets the path to the positive node.
        /// </summary>
        public IReadOnlyList<string> PosNodePath { get; }

        /// <summary>
        /// Gets the path to the negative node.
        /// </summary>
        public IReadOnlyList<string> NegNodePath { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNode">The node name.</param>
        public RealVoltageExport(IBiasingSimulation simulation, string posNode)
            : base(simulation)
        {
            posNode.ThrowIfNull(nameof(posNode));
            PosNodePath = new[] { posNode };
            NegNodePath = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNodePath">The node path.</param>
        public RealVoltageExport(IBiasingSimulation simulation, IEnumerable<string> posNodePath)
            : base(simulation)
        {
            PosNodePath = posNodePath.ThrowIfEmpty(nameof(posNodePath)).ToArray();
            NegNodePath = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNode">The positive node name.</param>
        /// <param name="negNode">The negative node name.</param>
        public RealVoltageExport(IBiasingSimulation simulation, string posNode, string negNode)
            : base(simulation)
        {
            posNode.ThrowIfNull(nameof(posNode));
            PosNodePath = new[] { posNode };
            if (posNode == null)
                NegNodePath = null;
            else
                NegNodePath = new[] { negNode };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNodePath">The positive node name.</param>
        /// <param name="negNodePath">The negative node name.</param>
        public RealVoltageExport(IBiasingSimulation simulation, IEnumerable<string> posNodePath, IEnumerable<string> negNodePath)
            : base(simulation)
        {
            PosNodePath = posNodePath.ThrowIfEmpty(nameof(posNodePath)).ToArray();
            NegNodePath = negNodePath?.ToArray();
            if (NegNodePath != null && NegNodePath.Count == 0)
                NegNodePath = null;
        }


        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            // Find the positive node variable
            var pos = GetVariable(PosNodePath);
            if (NegNodePath == null)
                Extractor = () => pos.Value;
            else
            {
                var neg = GetVariable(NegNodePath);
                Extractor = () => pos.Value - neg.Value;
            }
        }

        private IVariable<double> GetVariable(IReadOnlyList<string> path)
        {
            int last = path.Count - 1;
            IBiasingSimulationState state;
            if (path.Count > 1)
            {
                var entitiesBehavior = Simulation.EntityBehaviors[path[0]].GetValue<EntitiesBehavior>();
                for (int i = 1; i < last; i++)
                    entitiesBehavior = entitiesBehavior.LocalBehaviors[path[i]].GetValue<EntitiesBehavior>();;
                state = entitiesBehavior.GetState<IBiasingSimulationState>();
            }
            else
                state = Simulation.GetState<IBiasingSimulationState>();

            // Get the node
            if (!state.TryGetValue(path[last], out var result))
                throw new Diagnostics.ParameterNotFoundException(path[last]);
            return result;
        }
    }
}
