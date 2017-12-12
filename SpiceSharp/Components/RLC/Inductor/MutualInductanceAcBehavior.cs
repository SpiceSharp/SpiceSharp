using SpiceSharp.Behaviors;
using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="MutualInductance"/>
    /// </summary>
    public class MutualInductanceAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private MutualInductanceLoadBehavior load;

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement MUTbr1br2 { get; private set; }
        protected MatrixElement MUTbr2br1 { get; private set; }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            var mut = component as MutualInductance;
            load = GetBehavior<MutualInductanceLoadBehavior>(component);

            // Get behaviors
            var load1 = GetBehavior<InductorLoadBehavior>(mut.Inductor1);
            var load2 = GetBehavior<InductorLoadBehavior>(mut.Inductor2);

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            MUTbr1br2 = matrix.GetElement(load1.INDbrEq, load2.INDbrEq);
            MUTbr2br1 = matrix.GetElement(load2.INDbrEq, load1.INDbrEq);

            return true;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var cstate = ckt.State;
            Complex value = cstate.Laplace * load.MUTfactor;
            MUTbr1br2.Sub(value);
            MUTbr2br1.Sub(value);
        }
    }
}
