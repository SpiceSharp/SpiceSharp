using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents the mutual inductance
    /// </summary>
    public class MutualInductance : CircuitComponent
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("k"), SpiceName("coefficient"), SpiceInfo("Mutual inductance", IsPrincipal = true)]
        public Parameter<double> MUTcoupling { get; } = new Parameter<double>();
        [SpiceName("inductor1"), SpiceInfo("First coupled inductor")]
        public Parameter<string> MUTind1 { get; } = new Parameter<string>();
        [SpiceName("inductor2"), SpiceInfo("Second coupled inductor")]
        public Parameter<string> MUTind2 { get; } = new Parameter<string>();

        /// <summary>
        /// The factor
        /// </summary>
        public double MUTfactor { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private Inductor ind1, ind2;
        private const int INDflux = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the mutual inductance</param>
        public MutualInductance(string name) : base(name, 0) { }

        /// <summary>
        /// Setup the mutual inductance
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            // Find the inductors
            ind1 = ckt.Components[MUTind1] as Inductor;
            ind2 = ckt.Components[MUTind2] as Inductor;
            if (ind1 == null)
                throw new CircuitException($"{Name}: Could not find inductor '{MUTind1.Value}'");
            if (ind2 == null)
                throw new CircuitException($"{Name}: Could not find inductor '{MUTind2.Value}'");
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            MUTfactor = MUTcoupling * Math.Sqrt(ind1.INDinduct * ind2.INDinduct);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;

            if (!state.IsDc)
            {
                state.States[0][ind1.INDstate + INDflux] += MUTfactor * state.OldSolution[ind2.INDbrEq];
                state.States[0][ind2.INDstate + INDflux] += MUTfactor * state.OldSolution[ind1.INDbrEq];
            }
            state.Matrix[ind1.INDbrEq, ind2.INDbrEq] -= MUTfactor * ckt.Method.Slope;
            state.Matrix[ind2.INDbrEq, ind1.INDbrEq] -= MUTfactor * ckt.Method.Slope;
        }
    }
}
