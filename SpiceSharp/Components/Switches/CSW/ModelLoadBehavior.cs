using System;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CurrentSwitchBehaviors
{
    /// <summary>
    /// Load behavior for a <see cref="CurrentSwitchModel"/>
    /// </summary>
    public class ModelLoadBehavior : BaseLoadBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private ModelBaseParameters _mbp;

        /// <summary>
        /// Conductance while on
        /// </summary>
        public double OnConductance { get; protected set; }

        /// <summary>
        /// Conductance while off
        /// </summary>
        public double OffConductance { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public ModelLoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Get equation pointers
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(NodeMap nodes, Solver<double> solver)
        {
            // Do nothing
        }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _mbp = provider.GetParameterSet<ModelBaseParameters>("entity");
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            OnConductance = 1.0 / _mbp.OnResistance;
            OffConductance = 1.0 / _mbp.OffResistance;
        }
    }
}
