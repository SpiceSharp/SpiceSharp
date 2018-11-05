using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Noise behavior for a <see cref="Mosfet1"/>
    /// </summary>
    public class NoiseBehavior : Common.NoiseBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private ModelBaseParameters _mbp;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public NoiseBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            base.Setup(simulation, provider);

            // Get parameters
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");
        }
        
        /// <summary>
        /// Gets the oxide capacitance factor squared.
        /// </summary>
        /// <value>
        /// The oxide capacitance factor squared.
        /// </value>
        protected override double OxideCapSquared
        {
            get
            {
                double coxSquared;
                if (_mbp.OxideCapFactor > 0.0)
                    coxSquared = _mbp.OxideCapFactor;
                else
                    coxSquared = 3.9 * 8.854214871e-12 / 1e-7;
                coxSquared *= coxSquared;
                return coxSquared;
            }
        }
    }
}
