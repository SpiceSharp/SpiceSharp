using SpiceSharp.Diagnostics;
using SpiceSharp.Circuits;
using SpiceSharp.Components.RES;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.RES
{
    /// <summary>
    /// Temperature behavior for a <see cref="Components.Resistor"/>
    /// </summary>
    public class TemperatureBehavior : Behaviors.TemperatureBehavior
    {
        /// <summary>
        /// Necessary parameters
        /// </summary>
        ModelBaseParameters mbp;
        BaseParameters bp;

        /// <summary>
        /// Get the default conductance for this model
        /// </summary>
        public double RESconduct { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider"></param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
            if (!bp.RESresist.Given)
                mbp = provider.GetParameterSet<ModelBaseParameters>(1);
        }
        
        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Temperature(BaseSimulation sim)
        {
            double factor;
            double difference;
            double RESresist = bp.RESresist;

            // Default Value Processing for Resistor Instance
            if (!bp.REStemp.Given)
                bp.REStemp.Value = sim.State.Temperature;
            if (!bp.RESwidth.Given)
                bp.RESwidth.Value = mbp?.RESdefWidth ?? 0.0;

            if (mbp != null)
            {
                if (mbp.RESsheetRes.Given && (mbp.RESsheetRes != 0) && (bp.RESlength != 0))
                    RESresist = mbp.RESsheetRes * (bp.RESlength - mbp.RESnarrow) / (bp.RESwidth - mbp.RESnarrow);
                else
                {
                    CircuitWarning.Warning(this, $"{Name}: resistance=0, set to 1000");
                    RESresist = 1000;
                }

                difference = bp.REStemp - mbp.REStnom;
                factor = 1.0 + (mbp.REStempCoeff1) * difference + (mbp.REStempCoeff2) * difference * difference;
            }
            else
            {
                difference = bp.REStemp - 300.15;
                factor = 1.0;
            }

            RESconduct = 1.0 / (RESresist * factor);
        }
    }
}
