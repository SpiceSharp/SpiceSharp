using System;
using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Attributes;
using SpiceSharp.Components.VSRC;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.VSRC
{
    /// <summary>
    /// AC behavior for <see cref="Components.Voltagesource"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior, IConnectedBehavior
    {
        /// <summary>
        /// AC excitation vector
        /// </summary>
        public Complex VSRCac { get; protected set; }

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
        /// Properties
        /// </summary>
        [PropertyNameAttribute("v"), PropertyInfoAttribute("Complex voltage")]
        public Complex Voltage => VSRCac;
        [PropertyNameAttribute("i"), PropertyNameAttribute("c"), PropertyInfoAttribute("Complex current")]
        public Complex GetCurrent(State state)
        {
            return new Complex(
                state.Solution[VSRCbranch],
                state.iSolution[VSRCbranch]);
        }
        [PropertyNameAttribute("p"), PropertyInfoAttribute("Complex power")]
        public Complex GetPower(State state)
        {
            Complex v = new Complex(
                state.Solution[VSRCposNode] - state.Solution[VSRCnegNode],
                state.iSolution[VSRCposNode] - state.iSolution[VSRCnegNode]);
            Complex i = new Complex(
                state.Solution[VSRCbranch],
                state.iSolution[VSRCbranch]);
            return -v * Complex.Conjugate(i);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            var ap = provider.GetParameters<AcParameters>();

            // Calculate AC vector
            double radians = ap.VSRCacPhase * Circuit.CONSTPI / 180.0;
            VSRCac = new Complex(ap.VSRCacMag * Math.Cos(radians), ap.VSRCacMag * Math.Sin(radians));

            // Get behaviors
            var load = provider.GetBehavior<LoadBehavior>();
            VSRCbranch = load.VSRCbranch;
        }
        
        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            VSRCposNode = pins[0];
            VSRCnegNode = pins[1];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            VSRCposIbrptr = matrix.GetElement(VSRCposNode, VSRCbranch);
            VSRCibrPosptr = matrix.GetElement(VSRCbranch, VSRCposNode);
            VSRCnegIbrptr = matrix.GetElement(VSRCnegNode, VSRCbranch);
            VSRCibrNegptr = matrix.GetElement(VSRCbranch, VSRCnegNode);
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
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
            var cstate = sim.State;
            VSRCposIbrptr.Value.Real += 1.0;
            VSRCibrPosptr.Value.Real += 1.0;
            VSRCnegIbrptr.Value.Real -= 1.0;
            VSRCibrNegptr.Value.Real -= 1.0;
            cstate.Rhs[VSRCbranch] += VSRCac.Real;
            cstate.iRhs[VSRCbranch] += VSRCac.Imaginary;
        }
    }
}
