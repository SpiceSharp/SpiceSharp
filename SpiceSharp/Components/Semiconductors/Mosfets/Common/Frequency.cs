﻿using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// Small-signal behavior for a <see cref="Mosfet1" />.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="IFrequencyBehavior"/>
    public class Frequency : Behavior,
        IFrequencyBehavior
    {
        private readonly ElementSet<Complex> _elements;
        private readonly IComplexSimulationState _complex;
        private readonly Charges _charges = new Charges();

        /// <summary>
        /// The model parameters.
        /// </summary>
        protected readonly ModelParameters ModelParameters;

        /// <summary>
        /// The behavior that biased the mosfet.
        /// </summary>
        protected readonly IMosfetBiasingBehavior Behavior;

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
        public Frequency(string name, IComponentBindingContext context)
            : base(name) 
        {
            ModelParameters = context.ModelBehaviors.GetParameterSet<ModelParameters>();
            Behavior = context.Behaviors.GetValue<IMosfetBiasingBehavior>();
            _complex = context.GetState<IComplexSimulationState>();
            Variables = new MosfetVariables<Complex>(name, _complex, context.Nodes,
                !ModelParameters.DrainResistance.Equals(0.0) || !ModelParameters.SheetResistance.Equals(0.0) && Behavior.Parameters.DrainSquares > 0,
                !ModelParameters.SourceResistance.Equals(0.0) || !ModelParameters.SheetResistance.Equals(0.0) && Behavior.Parameters.SourceSquares > 0);
            _elements = new ElementSet<Complex>(_complex.Solver, Variables.GetMatrixLocations(_complex.Map));
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
            // Update the small-signal parameters
            _charges.Calculate(Behavior, ModelParameters);
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
        {
            int xnrm, xrev;

            if (Behavior.Mode < 0)
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
            var gateSourceOverlapCap = ModelParameters.GateSourceOverlapCapFactor * Behavior.Parameters.Width;
            var gateDrainOverlapCap = ModelParameters.GateDrainOverlapCapFactor * Behavior.Parameters.Width;
            var gateBulkOverlapCap = ModelParameters.GateBulkOverlapCapFactor * Behavior.Properties.EffectiveLength;

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
                Behavior.Properties.DrainConductance,
                new Complex(0.0, xgd + xgs + xgb),
                Behavior.Properties.SourceConductance,
                new Complex(Behavior.Gbd + Behavior.Gbs, xgb + xbd + xbs),
                new Complex(Behavior.Properties.DrainConductance + Behavior.Gds + Behavior.Gbd + xrev * (Behavior.Gm + Behavior.Gmbs), xgd + xbd),
                new Complex(Behavior.Properties.SourceConductance + Behavior.Gds + Behavior.Gbs + xnrm * (Behavior.Gm + Behavior.Gmbs), xgs + xbs),

                -Behavior.Properties.DrainConductance,

                -new Complex(0.0, xgb),
                -new Complex(0.0, xgd),
                -new Complex(0.0, xgs),

                -Behavior.Properties.SourceConductance,

                -new Complex(0.0, xgb),
                -new Complex(Behavior.Gbd, xbd),
                -new Complex(Behavior.Gbs, xbs),

                -Behavior.Properties.DrainConductance,
                new Complex((xnrm - xrev) * Behavior.Gm, -xgd),
                new Complex(-Behavior.Gbd + (xnrm - xrev) * Behavior.Gmbs, -xbd),
                -Behavior.Gds - xnrm * (Behavior.Gm + Behavior.Gmbs),

                -new Complex((xnrm - xrev) * Behavior.Gm, xgs),
                -Behavior.Properties.SourceConductance,
                -new Complex(Behavior.Gbs + (xnrm - xrev) * Behavior.Gmbs, xbs),
                -Behavior.Gds - xrev * (Behavior.Gm + Behavior.Gmbs));
        }
    }
}
