using SpiceSharp.Components.Subcircuits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export complex voltages.
    /// </summary>
    /// <seealso cref="Export{S, T}" />
    public class ComplexVoltageExport : Export<IFrequencySimulation, Complex>
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
        /// Gets the amplitude in decibels (dB).
        /// </summary>
        public double Decibels
        {
            get
            {
                var result = Value;
                return 10.0 * Math.Log10(result.Real * result.Real + result.Imaginary * result.Imaginary);
            }
        }

        /// <summary>
        /// Gets the phase in radians.
        /// </summary>
        public double Phase
        {
            get
            {
                var result = Value;
                return Math.Atan2(result.Imaginary, result.Real);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNode">The node name.</param>
        public ComplexVoltageExport(IFrequencySimulation simulation, string posNode)
            : base(simulation)
        {
            posNode.ThrowIfNull(nameof(posNode));
            PosNodePath = new[] { posNode };
            NegNodePath = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNodePath">The node path.</param>
        public ComplexVoltageExport(IFrequencySimulation simulation, IEnumerable<string> posNodePath)
            : base(simulation)
        {
            PosNodePath = posNodePath.ThrowIfEmpty(nameof(posNodePath)).ToArray();
            if (PosNodePath.Count == 0)
                throw new ArgumentNullException(nameof(posNodePath), "posNodePath cannot be null or empty.");
            NegNodePath = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNode">The positive node name.</param>
        /// <param name="negNode">The negative node name.</param>
        public ComplexVoltageExport(IFrequencySimulation simulation, string posNode, string negNode)
            : base(simulation)
        {
            posNode.ThrowIfNull(nameof(posNode));
            PosNodePath = new[] { posNode };
            if (negNode == null)
                NegNodePath = null;
            else
                NegNodePath = new[] { negNode };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNodePath">The positive node path.</param>
        /// <param name="negNodePath">The negative node path.</param>
        public ComplexVoltageExport(IFrequencySimulation simulation, IEnumerable<string> posNodePath, IEnumerable<string> negNodePath)
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

        private IVariable<Complex> GetVariable(IReadOnlyList<string> path)
        {
            int last = path.Count - 1;
            IComplexSimulationState state;
            if (path.Count > 1)
            {
                var entitiesBehavior = Simulation.EntityBehaviors[path[0]].GetValue<EntitiesBehavior>();
                for (int i = 1; i < last; i++)
                    entitiesBehavior = entitiesBehavior.LocalBehaviors[path[i]].GetValue<EntitiesBehavior>();
                state = entitiesBehavior.GetState<IComplexSimulationState>();
            }
            else
                state = Simulation.GetState<IComplexSimulationState>();

            // Get the node
            if (!state.TryGetValue(path[last], out var result))
                throw new Diagnostics.ParameterNotFoundException(path[last]);
            return result;
        }
    }
}
