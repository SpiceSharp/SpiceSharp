using SpiceSharp.Circuits;
using SpiceSharp.Components.VSW;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.VSW
{
    /// <summary>
    /// Load behavior for a <see cref="Components.VoltageSwitchModel"/>
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
        public double VSWonConduct { get; protected set; }

        /// <summary>
        /// Conductance while off
        /// </summary>
        public double VSWoffConduct { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public ModelLoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
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
            VSWonConduct = 1.0 / mbp.VSWon;
            VSWoffConduct = 1.0 / mbp.VSWoff;
        }
    }
}
