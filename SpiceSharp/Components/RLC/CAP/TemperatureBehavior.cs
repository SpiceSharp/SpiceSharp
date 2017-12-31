using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.CAP;

namespace SpiceSharp.Behaviors.CAP
{
    /// <summary>
    /// Temperature behavior for a <see cref="Components.Capacitor"/>
    /// </summary>
    public class TemperatureBehavior : Behaviors.TemperatureBehavior, IModelBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
        /// </summary>
        ModelBaseParameters mbp;
        BaseParameters bp;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <param name="pool">Pool</param>
        public override void Setup(ParametersCollection parameters, BehaviorPool pool)
        {
            bp = parameters.Get<BaseParameters>();
        }

        /// <summary>
        /// Setup model parameters and behaviors
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <param name="pool">Behaviors</param>
        public void SetupModel(ParametersCollection parameters, BehaviorPool pool)
        {
            mbp = parameters.Get<ModelBaseParameters>();
        }

        /// <summary>
        /// Execute the behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            if (!bp.CAPcapac.Given)
            {
                if (mbp == null)
                    throw new CircuitException("No model specified");

                double width = bp.CAPwidth.Given ? bp.CAPwidth.Value : mbp.CAPdefWidth.Value;
                bp.CAPcapac.Value = mbp.CAPcj *
                    (bp.CAPwidth - mbp.CAPnarrow) *
                    (bp.CAPlength - mbp.CAPnarrow) +
                    mbp.CAPcjsw * 2 * (
                    (bp.CAPlength - mbp.CAPnarrow) +
                    (bp.CAPwidth - mbp.CAPnarrow));
            }
        }
    }
}
