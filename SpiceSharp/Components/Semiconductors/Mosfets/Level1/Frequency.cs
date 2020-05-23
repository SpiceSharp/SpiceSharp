using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.ParameterSets;

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
        /// The variables used by the transistor.
        /// </summary>
        protected MosfetVariables<Complex> Variables;

        /// <include file='../common/docs.xml' path='docs/members/GateSourceCapacitance/*'/>
        [ParameterName("cgs"), ParameterInfo("Gate-source capacitance", Units = "F")]
        public double Cgs => _charges.Cgs;

        /// <include file='../common/docs.xml' path='docs/members/GateDrainCapacitance/*'/>
        [ParameterName("cgd"), ParameterInfo("Gate-drain capacitance", Units = "F")]
        public double Cgd => _charges.Cgd;

        /// <include file='../common/docs.xml' path='docs/members/GateBulkCapacitance/*'/>
        [ParameterName("cgb"), ParameterInfo("Gate-bulk capacitance", Units = "F")]
        public double Cgb => _charges.Cgb;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="context">The binding context.</param>
        public Frequency(string name, ComponentBindingContext context)
            : base(name, context) 
        {
            _complex = context.GetState<IComplexSimulationState>();
            Variables = new MosfetVariables<Complex>(name, _complex, context.Nodes,
                !ModelParameters.DrainResistance.Equals(0.0) || !ModelParameters.SheetResistance.Equals(0.0) && Parameters.DrainSquares > 0,
                !ModelParameters.SourceResistance.Equals(0.0) || !ModelParameters.SheetResistance.Equals(0.0) && Parameters.SourceSquares > 0);
            _elements = new ElementSet<Complex>(_complex.Solver, Variables.GetMatrixLocations(_complex.Map));
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
            // Update the small-signal parameters
            _charges.Update(Mode, Vgs, Vds, Vbs,
                ModelParameters.MosfetType * Von,
                ModelParameters.MosfetType * Vdsat,
                ModelParameters,
                Properties);
        }

        /// <inheritdoc/>
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
                new Complex(Gbd + Gbs, xgb + xbd + xbs),
                new Complex(Properties.DrainConductance + Gds + Gbd + xrev * (Gm + Gmbs), xgd + xbd),
                new Complex(Properties.SourceConductance + Gds + Gbs + xnrm * (Gm + Gmbs), xgs + xbs),

                -Properties.DrainConductance,

                -new Complex(0.0, xgb),
                -new Complex(0.0, xgd),
                -new Complex(0.0, xgs),

                -Properties.SourceConductance,

                -new Complex(0.0, xgb),
                -new Complex(Gbd, xbd),
                -new Complex(Gbs, xbs),

                -Properties.DrainConductance,
                new Complex((xnrm - xrev) * Gm, -xgd),
                new Complex(-Gbd + (xnrm - xrev) * Gmbs, -xbd),
                -Gds - xnrm * (Gm + Gmbs),

                -new Complex((xnrm - xrev) * Gm, xgs),
                -Properties.SourceConductance,
                -new Complex(Gbs + (xnrm - xrev) * Gmbs, xbs),
                -Gds - xrev * (Gm + Gmbs));
        }
    }
}
