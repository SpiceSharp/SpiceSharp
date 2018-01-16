using System;
using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Attributes;
using SpiceSharp.Components.VSRC;

namespace SpiceSharp.Behaviors.VSRC
{
    /// <summary>
    /// AC behavior for <see cref="Components.Voltagesource"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("acreal"), SpiceInfo("A.C. real part")]
        public double GetAcReal() => VSRCac.Real;
        [SpiceName("acimag"), SpiceInfo("A.C. imaginary part")]
        public double GetAcImag() => VSRCac.Imaginary;

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
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create an export method for AC analysis
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns></returns>
        public override Func<State, Complex> CreateAcExport(string property)
        {
            switch (property)
            {
                case "v": return (State state) => new Complex(state.Solution[VSRCposNode] - state.Solution[VSRCnegNode], state.iSolution[VSRCposNode] - state.Solution[VSRCnegNode]);
                case "i": return (State state) => new Complex(state.Solution[VSRCbranch], state.iSolution[VSRCbranch]);
                case "p": return (State state) =>
                {
                    Complex voltage = new Complex(state.Solution[VSRCposNode] - state.Solution[VSRCnegNode], state.iSolution[VSRCposNode] - state.Solution[VSRCnegNode]);
                    Complex current = new Complex(state.Solution[VSRCbranch], state.iSolution[VSRCbranch]);
                    return voltage * Complex.Conjugate(current);
                };
                default: return null;
            }
        }

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
        /// Execute AC behavior
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
