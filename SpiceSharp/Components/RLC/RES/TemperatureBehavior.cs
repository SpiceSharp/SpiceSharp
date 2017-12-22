using SpiceSharp.Diagnostics;
using SpiceSharp.Components;
using SpiceSharp.Attributes;
using SpiceSharp.Circuits;
using SpiceSharp.Components.RES;

namespace SpiceSharp.Behaviors.RES
{
    /// <summary>
    /// Temperature behavior for a <see cref="Resistor"/>
    /// </summary>
    public class TemperatureBehavior : Behaviors.TemperatureBehavior, IModelBehavior
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
        /// Name of the component
        /// </summary>
        Identifier name;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <param name="pool">Pool of all behaviors</param>
        public override void Setup(ParametersCollection parameters, BehaviorPool pool)
        {
            bp = parameters.Get<BaseParameters>();
        }

        /// <summary>
        /// Setup the model of the behavior
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <param name="pool">Pool of all behaviors</param>
        public void SetupModel(ParametersCollection parameters, BehaviorPool pool)
        {
            mbp = parameters.Get<ModelBaseParameters>();
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double factor;
            double difference;
            double RESresist = bp.RESresist;

            // Default Value Processing for Resistor Instance
            if (!bp.REStemp.Given)
                bp.REStemp.Value = ckt.State.Temperature;
            if (!bp.RESwidth.Given)
                bp.RESwidth.Value = mbp?.RESdefWidth ?? 0.0;

            if (mbp != null)
            {
                if (mbp.RESsheetRes.Given && (mbp.RESsheetRes != 0) && (bp.RESlength != 0))
                    RESresist = mbp.RESsheetRes * (bp.RESlength - mbp.RESnarrow) / (bp.RESwidth - mbp.RESnarrow);
                else
                {
                    CircuitWarning.Warning(this, $"{name}: resistance=0, set to 1000");
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
