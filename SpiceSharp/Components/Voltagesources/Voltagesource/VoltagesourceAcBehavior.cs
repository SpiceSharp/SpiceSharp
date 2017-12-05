using System;
using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="Voltagesource"/>
    /// </summary>
    public class VoltageSourceLoadAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(ICircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var vsrc = ComponentTyped<Voltagesource>();
            double radians = vsrc.VSRCacPhase * Circuit.CONSTPI / 180.0;
            vsrc.VSRCac = new Complex(vsrc.VSRCacMag * Math.Cos(radians), vsrc.VSRCacMag * Math.Sin(radians));
        }

        /// <summary>
        /// Execute AC behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var vsrc = ComponentTyped<Voltagesource>();

            var cstate = ckt.State;

            vsrc.VSRCposIbrptr.Value.Real += 1.0;
            vsrc.VSRCibrPosptr.Value.Real += 1.0;
            vsrc.VSRCnegIbrptr.Value.Real -= 1.0;
            vsrc.VSRCibrNegptr.Value.Real -= 1.0;
            cstate.Rhs[vsrc.VSRCbranch] += vsrc.VSRCac.Real;
            cstate.iRhs[vsrc.VSRCbranch] += vsrc.VSRCac.Imaginary;
        }
    }
}
