using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for a <see cref="Inductor"/>
    /// </summary>
    public class InductorLoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var ind = ComponentTyped<Inductor>();
            var state = ckt.State;
            var rstate = state.Real;

            // Initialize
            if (state.UseIC && ind.INDinitCond.Given)
                state.States[0][ind.INDstate + Inductor.INDflux] = ind.INDinduct * ind.INDinitCond;
            else
                state.States[0][ind.INDstate + Inductor.INDflux] = ind.INDinduct * rstate.OldSolution[ind.INDbrEq];

            // Handle mutual inductances
            ind.UpdateMutualInductances(ckt);

            // Finally load the Y-matrix
            // Note that without an integration method, the result will be a short circuit
            if (ckt.Method != null)
            {
                var result = ckt.Method.Integrate(state, ind.INDstate + Inductor.INDflux, ind.INDinduct);
                rstate.Rhs[ind.INDbrEq] += result.Ceq;
                rstate.Matrix[ind.INDbrEq, ind.INDbrEq] -= result.Geq;
            }

            rstate.Matrix[ind.INDposNode, ind.INDbrEq] += 1;
            rstate.Matrix[ind.INDnegNode, ind.INDbrEq] -= 1;
            rstate.Matrix[ind.INDbrEq, ind.INDposNode] += 1;
            rstate.Matrix[ind.INDbrEq, ind.INDnegNode] -= 1;
        }
    }
}
