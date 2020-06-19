using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.CurrentSources
{
    /// <summary>
    /// Behavior of a currentsource in AC analysis
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="IFrequencyBehavior"/>
    public class Frequency : Biasing,
        IFrequencyBehavior
    {
        private readonly IComplexSimulationState _complex;
        private readonly OnePort<Complex> _variables;
        private readonly ElementSet<Complex> _elements;

        /// <include file='../../Common/docs.xml' path='docs/members[@name="frequency"]/Voltage/*'/>
        [ParameterName("v"), ParameterInfo("Complex voltage")]
        public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <include file='../../Common/docs.xml' path='docs/members[@name="frequency"]/Power/*'/>
        [ParameterName("p"), ParameterInfo("Complex power")]
        public Complex ComplexPower
        {
            get
            {
                var v = _variables.Positive.Value - _variables.Negative.Value;
                return -v * Complex.Conjugate(Parameters.Phasor);
            }
        }

        /// <include file='../../Common/docs.xml' path='docs/members[@name="frequency"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterName("i_c"), ParameterInfo("Complex current")]
        public Complex ComplexCurrent => Parameters.Phasor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="context">The binding context.</param>
        public Frequency(string name, IComponentBindingContext context)
            : base(name, context)
        {
            _complex = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(_complex, context);
            _elements = new ElementSet<Complex>(_complex.Solver, null, _variables.GetRhsIndices(_complex.Map));
        }

        void IFrequencyBehavior.InitializeParameters()
        {
            Parameters.UpdatePhasor();
        }

        void IFrequencyBehavior.Load()
        {
            // NOTE: Spice 3f5's documentation is IXXXX POS NEG VALUE but in the code it is IXXXX NEG POS VALUE
            // I solved it by inverting the current when loading the rhs vector
            var value = Parameters.Phasor;
            _elements.Add(-value, value);
        }
    }
}