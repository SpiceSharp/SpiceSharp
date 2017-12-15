using System;
using System.Numerics;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Behaviors.ISRC
{
    /// <summary>
    /// Behavior of a currentsource in AC analysis
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("acmag"), SpiceInfo("A.C. magnitude value")]
        public Parameter ISRCacMag { get; } = new Parameter();
        [SpiceName("acphase"), SpiceInfo("A.C. phase value")]
        public Parameter ISRCacPhase { get; } = new Parameter();
        [SpiceName("ac"), SpiceInfo("A.C. magnitude, phase vector")]
        public void SetAc(double[] ac)
        {
            switch (ac.Length)
            {
                case 2: ISRCacPhase.Set(ac[1]); goto case 1;
                case 1: ISRCacMag.Set(ac[0]); break;
                case 0: ISRCacMag.Set(0.0); break;
                default:
                    throw new BadParameterException("ac");
            }
        }

        /// <summary>
        /// Nodes
        /// </summary>
        private int ISRCposNode, ISRCnegNode;
        private Complex ISRCac;

        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(Entity component, Circuit ckt)
        {
            base.Setup(component, ckt);

            var isrc = component as Currentsource;
            double radians = ISRCacPhase * Circuit.CONSTPI / 180.0;
            ISRCac = new Complex(ISRCacMag * Math.Cos(radians), ISRCacMag * Math.Sin(radians));

            // Copy nodes
            ISRCposNode = isrc.ISRCposNode;
            ISRCnegNode = isrc.ISRCnegNode;
        }

        /// <summary>
        /// Execute AC behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var cstate = ckt.State;
            cstate.Rhs[ISRCposNode] += ISRCac.Real;
            cstate.iRhs[ISRCposNode] += ISRCac.Imaginary;
            cstate.Rhs[ISRCnegNode] -= ISRCac.Real;
            cstate.iRhs[ISRCnegNode] -= ISRCac.Imaginary;
        }
    }
}
