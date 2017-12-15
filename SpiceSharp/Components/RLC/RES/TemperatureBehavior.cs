using SpiceSharp.Diagnostics;
using SpiceSharp.Components;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.RES
{
    /// <summary>
    /// Temperature behaviour for a <see cref="Resistor"/>
    /// </summary>
    public class TemperatureBehavior : Behaviors.TemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("temp"), SpiceInfo("Instance operating temperature", Interesting = false)]
        public double RES_TEMP
        {
            get => REStemp - Circuit.CONSTCtoK;
            set => REStemp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter REStemp { get; } = new Parameter(300.15);
        [SpiceName("w"), SpiceInfo("Width", Interesting = false)]
        public Parameter RESwidth { get; } = new Parameter();
        [SpiceName("l"), SpiceInfo("Length", Interesting = false)]
        public Parameter RESlength { get; } = new Parameter();

        /// <summary>
        /// Get the default conductance for this model
        /// </summary>
        public double RESresist { get; protected set; }
        public double RESconduct { get; protected set; }

        /// <summary>
        /// Name of the component
        /// </summary>
        private CircuitIdentifier name;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component"></param>
        /// <param name="ckt"></param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var res = component as Resistor;
            if (res.Model == null)
            {
                modeltemp = null;
                return;
            }
            modeltemp = GetBehavior<ModelTemperatureBehavior>(res.Model);
            name = res.Name;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double factor;
            double difference;

            // Default Value Processing for Resistor Instance
            if (!REStemp.Given)
                REStemp.Value = ckt.State.Temperature;
            if (!RESwidth.Given)
                RESwidth.Value = modeltemp?.RESdefWidth ?? 0.0;

            if (modeltemp == null)
                throw new CircuitException("No modeltemp specified");
            if (modeltemp.RESsheetRes.Given && (modeltemp.RESsheetRes != 0) && (RESlength != 0))
                RESresist = modeltemp.RESsheetRes * (RESlength - modeltemp.RESnarrow) / (RESwidth - modeltemp.RESnarrow);
            else
            {
                CircuitWarning.Warning(this, $"{name}: resistance=0, set to 1000");
                RESresist = 1000;
            }

            if (modeltemp != null)
            {
                difference = REStemp - modeltemp.REStnom;
                factor = 1.0 + (modeltemp.REStempCoeff1) * difference + (modeltemp.REStempCoeff2) * difference * difference;
            }
            else
            {
                difference = REStemp - 300.15;
                factor = 1.0;
            }

            RESconduct = 1.0 / (RESresist * factor);
        }
    }
}
