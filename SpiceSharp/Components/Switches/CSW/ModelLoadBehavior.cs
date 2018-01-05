using SpiceSharp.Circuits;
using SpiceSharp.Components.CSW;

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
        /// Constants
        /// </summary>
        public const double CSWdefOnResistance = 1.0;
        public const double CSWdefOffResistance = 1e12;

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
            CSWonConduct = 1.0 / mbp.CSWon;
            CSWoffConduct = 1.0 / mbp.CSWoff;
        }
        
        /// <summary>
        /// Load behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            // Do nothing
        }
    }
}
