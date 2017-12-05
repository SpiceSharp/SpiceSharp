using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="Resistor"/>
    /// </summary>
    public class ResistorTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var res = ComponentTyped<Resistor>();
            double factor;
            double difference;
            ResistorModel model = res.Model as ResistorModel;

            // Default Value Processing for Resistor Instance
            if (!res.REStemp.Given)
                res.REStemp.Value = ckt.State.Temperature;
            if (!res.RESwidth.Given)
                res.RESwidth.Value = model?.RESdefWidth ?? 0.0;
            if (!res.RESresist.Given)
            {
                if (model == null)
                    throw new CircuitException("No model specified");
                if (model.RESsheetRes.Given && (model.RESsheetRes != 0) && (res.RESlength != 0))
                    res.RESresist.Value = model.RESsheetRes * (res.RESlength - model.RESnarrow) / (res.RESwidth - model.RESnarrow);
                else
                {
                    CircuitWarning.Warning(this, $"{res.Name}: resistance=0, set to 1000");
                    res.RESresist.Value = 1000;
                }
            }

            if (model != null)
            {
                difference = res.REStemp - model.REStnom;
                factor = 1.0 + (model.REStempCoeff1) * difference + (model.REStempCoeff2) * difference * difference;
            }
            else
            {
                difference = res.REStemp - 300.15;
                factor = 1.0;
            }

            res.RESconduct = 1.0 / (res.RESresist * factor);
        }
    }
}
