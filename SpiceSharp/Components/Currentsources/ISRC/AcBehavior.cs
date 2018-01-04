using System;
using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Components.ISRC;

namespace SpiceSharp.Behaviors.ISRC
{
    /// <summary>
    /// Behavior of a currentsource in AC analysis
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        AcParameters ap;

        /// <summary>
        /// Nodes
        /// </summary>
        int ISRCposNode, ISRCnegNode;
        Complex ISRCac;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcBehavior(Identifier name) : base(name) { }

        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            ap = provider.GetParameters<AcParameters>();

            // Calculate the AC vector
            double radians = ap.ISRCacPhase * Circuit.CONSTPI / 180.0;
            ISRCac = new Complex(ap.ISRCacMag * Math.Cos(radians), ap.ISRCacMag * Math.Sin(radians));
        }
        
        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            ISRCposNode = pins[0];
            ISRCnegNode = pins[1];
        }
        
        /// <summary>
        /// Execute AC behavior
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
