using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.Mosfets.Level1
{
    /// <summary>
    /// Small-signal behavior for a <see cref="Mosfet1" />.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IFrequencyBehavior"/>
    public class Frequency : Biasing,
        IFrequencyBehavior
    {
        private readonly ElementSet<Complex> _elements;
        private readonly IComplexSimulationState _complex;
        private readonly MosfetCharges _charges = new MosfetCharges();

        /// <summary>
        /// The variables for the small-signal behavior.
        /// </summary>
        protected readonly MosfetVariables<Complex> Variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="context">The binding context.</param>
        public Frequency(string name, ComponentBindingContext context) : base(name, context) 
        {
            _complex = context.GetState<IComplexSimulationState>();
            Variables = new MosfetVariables<Complex>(name, _complex, context.Nodes,
                !ModelParameters.DrainResistance.Equals(0.0) || !ModelParameters.SheetResistance.Equals(0.0) && Parameters.DrainSquares > 0,
                !ModelParameters.SourceResistance.Equals(0.0) || !ModelParameters.SheetResistance.Equals(0.0) && Parameters.SourceSquares > 0);
            _elements = new ElementSet<Complex>(_complex.Solver, Variables.GetMatrixLocations(_complex.Map));
        }

        void IFrequencyBehavior.InitializeParameters()
        {
            // Update the small-signal parameters
            _charges.Update(Mode, Vgs, Vds, Vbs,
                ModelParameters.MosfetType * Von,
                ModelParameters.MosfetType * Vdsat,
                ModelParameters,
                Properties);
        }

        void IFrequencyBehavior.Load()
        {
            int xnrm, xrev;

            if (Mode < 0)
            {
                xnrm = 0;
                xrev = 1;
            }
            else
            {
                xnrm = 1;
                xrev = 0;
            }

            // Charge oriented model parameters
            var gateSourceOverlapCap = ModelParameters.GateSourceOverlapCapFactor * Parameters.Width;
            var gateDrainOverlapCap = ModelParameters.GateDrainOverlapCapFactor * Parameters.Width;
            var gateBulkOverlapCap = ModelParameters.GateBulkOverlapCapFactor * Properties.EffectiveLength;

            // Meyer"s model parameters
            var capgs = _charges.Cgs * 2 + gateSourceOverlapCap;
            var capgd = _charges.Cgd * 2 + gateDrainOverlapCap;
            var capgb = _charges.Cgb * 2 + gateBulkOverlapCap;
            var xgs = capgs * _complex.Laplace.Imaginary;
            var xgd = capgd * _complex.Laplace.Imaginary;
            var xgb = capgb * _complex.Laplace.Imaginary;
            var xbd = _charges.Cbd * _complex.Laplace.Imaginary;
            var xbs = _charges.Cbs * _complex.Laplace.Imaginary;

            // Load Y-matrix
            _elements.Add(
                Properties.DrainConductance,
                new Complex(0.0, xgd + xgs + xgb),
                Properties.SourceConductance,
                new Complex(CondBd + CondBs, xgb + xbd + xbs),
                new Complex(Properties.DrainConductance + CondDs + CondBd + xrev * (Gm + Gmbs), xgd + xbd),
                new Complex(Properties.SourceConductance + CondDs + CondBs + xnrm * (Gm + Gmbs), xgs + xbs),

                -Properties.DrainConductance,

                -new Complex(0.0, xgb),
                -new Complex(0.0, xgd),
                -new Complex(0.0, xgs),

                -Properties.SourceConductance,

                -new Complex(0.0, xgb),
                -new Complex(CondBd, xbd),
                -new Complex(CondBs, xbs),

                -Properties.DrainConductance,
                new Complex((xnrm - xrev) * Gm, -xgd),
                new Complex(-CondBd + (xnrm - xrev) * Gmbs, -xbd),
                -CondDs - xnrm * (Gm + Gmbs),

                -new Complex((xnrm - xrev) * Gm, xgs),
                -Properties.SourceConductance,
                -new Complex(CondBs + (xnrm - xrev) * Gmbs, xbs),
                -CondDs - xrev * (Gm + Gmbs));
        }
    }
}
