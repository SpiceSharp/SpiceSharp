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
        public MutualInductance(string name) : base(name, 0)
        {
            // Make sure mutual inductances are evaluated AFTER inductors
            Priority = -1;
        }

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

            // Register our method for updating mutual inductance flux
            ind1.UpdateMutualInductance += UpdateMutualInductance;
        }

        /// <summary>
        /// Get the model
        /// </summary>
        /// <returns></returns>
        public override CircuitModel GetModel() => null;

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            MUTfactor = MUTcoupling * Math.Sqrt(ind1.INDinduct * ind2.INDinduct);
        }

        /// <summary>
        /// Update inductor 2
        /// </summary>
        /// <param name="sender">Inductor 2</param>
        /// <param name="ckt">The circuit</param>
        private void UpdateMutualInductance(Inductor sender, Circuit ckt)
        {
            var state = ckt.State;
            var rstate = ckt.State.Real;

            if (sender == ind1)
            {
                state.States[0][ind1.INDstate + INDflux] += MUTfactor * rstate.OldSolution[ind2.INDbrEq];
                rstate.Matrix[ind1.INDbrEq, ind2.INDbrEq] -= MUTfactor * ckt.Method.Slope;
            }
            else
            {
                state.States[0][ind2.INDstate + INDflux] += MUTfactor * rstate.OldSolution[ind1.INDbrEq];
                rstate.Matrix[ind2.INDbrEq, ind1.INDbrEq] -= MUTfactor * ckt.Method.Slope;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            // Don't need to do anything here
        }
    }
}
