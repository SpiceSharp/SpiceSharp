using SpiceSharp.Components;
using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.MUT
{
    /// <summary>
    /// AC behaviour for <see cref="MutualInductance"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior load;

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
        public override void Setup(Entity component, Circuit ckt)
        {
            var mut = component as MutualInductance;
            
            // Get behaviors
            load = GetBehavior<LoadBehavior>(component);
            var load1 = GetBehavior<IND.LoadBehavior>(mut.Inductor1);
            var load2 = GetBehavior<IND.LoadBehavior>(mut.Inductor2);

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            MUTbr1br2 = matrix.GetElement(load1.INDbrEq, load2.INDbrEq);
            MUTbr2br1 = matrix.GetElement(load2.INDbrEq, load1.INDbrEq);
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
