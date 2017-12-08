using System;
using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Behavior of a currentsource in AC analysis
    /// </summary>
    public class CurrentsourceAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var isrc = ComponentTyped<Currentsource>();
            double radians = isrc.ISRCacPhase * Circuit.CONSTPI / 180.0;
            isrc.ISRCac = new Complex(isrc.ISRCacMag * Math.Cos(radians), isrc.ISRCacMag * Math.Sin(radians));
        }

        /// <summary>
        /// Execute AC behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var source = ComponentTyped<Currentsource>();

            var cstate = ckt.State;
            cstate.Rhs[source.ISRCposNode] += source.ISRCac.Real;
            cstate.iRhs[source.ISRCposNode] += source.ISRCac.Imaginary;
            cstate.Rhs[source.ISRCnegNode] -= source.ISRCac.Real;
            cstate.iRhs[source.ISRCnegNode] -= source.ISRCac.Imaginary;
        }
    }
}
