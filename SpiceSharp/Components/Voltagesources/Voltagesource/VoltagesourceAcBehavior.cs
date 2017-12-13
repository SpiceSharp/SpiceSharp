using System;
using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Parameters;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="Voltagesource"/>
    /// </summary>
    public class VoltageSourceLoadAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("acmag"), SpiceInfo("A.C. Magnitude")]
        public Parameter VSRCacMag { get; } = new Parameter();
        [SpiceName("acphase"), SpiceInfo("A.C. Phase")]
        public Parameter VSRCacPhase { get; } = new Parameter();
        [SpiceName("ac"), SpiceInfo("A.C. magnitude, phase vector")]
        public void SetAc(Circuit ckt, double[] ac)
        {
            switch (ac?.Length ?? -1)
            {
                case 2: VSRCacPhase.Set(ac[1]); goto case 1;
                case 1: VSRCacMag.Set(ac[0]); break;
                case 0: VSRCacMag.Set(0.0); break;
                default:
                    throw new BadParameterException("ac");
            }
        }
        [SpiceName("acreal"), SpiceInfo("A.C. real part")]
        public double GetAcReal(Circuit ckt) => VSRCac.Real;
        [SpiceName("acimag"), SpiceInfo("A.C. imaginary part")]
        public double GetAcImag(Circuit ckt) => VSRCac.Imaginary;
        public Complex VSRCac;

        /// <summary>
        /// Nodes
        /// </summary>
        protected int VSRCposNode, VSRCnegNode, VSRCbranch;

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement VSRCposIbrptr { get; private set; }
        protected MatrixElement VSRCnegIbrptr { get; private set; }
        protected MatrixElement VSRCibrPosptr { get; private set; }
        protected MatrixElement VSRCibrNegptr { get; private set; }
        protected MatrixElement VSRCibrIbrptr { get; private set; }

        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var vsrc = component as Voltagesource;

            // Get behaviors
            var load = GetBehavior<VoltagesourceLoadBehavior>(component);

            // Get nodes
            VSRCposNode = vsrc.VSRCposNode;
            VSRCnegNode = vsrc.VSRCnegNode;
            VSRCbranch = load.VSRCbranch;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            VSRCposIbrptr = matrix.GetElement(VSRCposNode, VSRCbranch);
            VSRCibrPosptr = matrix.GetElement(VSRCbranch, VSRCposNode);
            VSRCnegIbrptr = matrix.GetElement(VSRCnegNode, VSRCbranch);
            VSRCibrNegptr = matrix.GetElement(VSRCbranch, VSRCnegNode);

            double radians = VSRCacPhase * Circuit.CONSTPI / 180.0;
            VSRCac = new Complex(VSRCacMag * Math.Cos(radians), VSRCacMag * Math.Sin(radians));
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            VSRCposIbrptr = null;
            VSRCibrPosptr = null;
            VSRCnegIbrptr = null;
            VSRCibrNegptr = null;
        }

        /// <summary>
        /// Execute AC behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var cstate = ckt.State;
            VSRCposIbrptr.Value.Real += 1.0;
            VSRCibrPosptr.Value.Real += 1.0;
            VSRCnegIbrptr.Value.Real -= 1.0;
            VSRCibrNegptr.Value.Real -= 1.0;
            cstate.Rhs[VSRCbranch] += VSRCac.Real;
            cstate.iRhs[VSRCbranch] += VSRCac.Imaginary;
        }
    }
}
