using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Inductors
{
    /// <summary>
    /// DC biasing behavior for an <see cref="Inductor" />
    /// </summary>
    /// <seealso cref="Temperature"/>
    /// <seealso cref="IBiasingBehavior" />
    /// <seealso cref="IBranchedBehavior{T}" />
    [BehaviorFor(typeof(Inductor)), AddBehaviorIfNo(typeof(IBiasingBehavior))]
    [GeneratedParameters]
    public partial class Biasing : Temperature,
        IBiasingBehavior,
        IBranchedBehavior<double>
    {
        private readonly ElementSet<double> _elements;
        private readonly OnePort<double> _variables;

        /// <inheritdoc/>
        public IVariable<double> Branch { get; }

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Current/*'/>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Current")]
        public double Current => Branch.Value;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Voltage/*'/>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double Voltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Power/*'/>
        [ParameterName("p"), ParameterInfo("Power")]
        public double Power => -Voltage * Current;

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Biasing(IComponentBindingContext context) 
            : base(context)
        {
            context.Nodes.CheckNodes(2);
            var state = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(state, context);
            Branch = state.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);

            int pos = state.Map[_variables.Positive];
            int neg = state.Map[_variables.Negative];
            int br = state.Map[Branch];
            _elements = new ElementSet<double>(state.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br),
                new MatrixLocation(br, neg),
                new MatrixLocation(br, pos));
        }

        /// <inheritdoc/>
        public virtual void Load()
        {
            _elements.Add(1, -1, -1, 1);
        }
    }
}