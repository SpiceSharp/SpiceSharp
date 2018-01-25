using SpiceSharp.Components.CSW;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.CSW
{
    /// <summary>
    /// Load behavior for a <see cref="Components.CurrentSwitchModel"/>
    /// </summary>
    public class ModelLoadBehavior : Behaviors.LoadBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        ModelBaseParameters mbp;

        /// <summary>
        /// Conductance while on
        /// </summary>
        public double CSWonConduct { get; protected set; }

        /// <summary>
        /// Conductance while off
        /// </summary>
        public double CSWoffConduct { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public ModelLoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            mbp = provider.GetParameters<ModelBaseParameters>();
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            CSWonConduct = 1.0 / mbp.CSWon;
            CSWoffConduct = 1.0 / mbp.CSWoff;
        }
    }
}
