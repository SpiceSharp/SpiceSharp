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
            var rstate = state;

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
                ind.INDibrIbrptr.Sub(result.Geq);
            }

            ind.INDposIbrptr.Add(1.0);
            ind.INDnegIbrptr.Sub(1.0);
            ind.INDibrPosptr.Add(1.0);
            ind.INDibrNegptr.Sub(1.0);
        }
    }
}
