using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.VoltageDelays
{
    /// <summary>
    /// Biasing behavior for a <see cref="VoltageDelay" />.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IBranchedBehavior{T}"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="Parameters"/>
    [BehaviorFor(typeof(VoltageDelay)), AddBehaviorIfNo(typeof(IBiasingBehavior))]
    [GeneratedParameters]
    public partial class Biasing : Behavior,
        IBiasingBehavior,
        IBranchedBehavior<double>,
        IParameterized<Parameters>
    {
        /// <summary>
        /// Gets the variables.
        /// </summary>
        protected TwoPort<double> Variables { get; }

        /// <inheritdoc/>
        public Parameters Parameters { get; }

        /// <inheritdoc/>
        public IVariable<double> Branch { get; }

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> BiasingElements { get; }

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Voltage/*'/>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Voltage accross the supply")]
        public double Voltage => Variables.Right.Positive.Value - Variables.Right.Negative.Value;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Power/*'/>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power supplied by the source")]
        public double Power => Voltage * -Current;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Current/*'/>
        [ParameterName("c"), ParameterName("i"), ParameterName("i_r"), ParameterInfo("Current through current source")]
        public double Current => Branch.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Biasing(IComponentBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(4);

            Parameters = context.GetParameterSet<Parameters>();
            var state = context.GetState<IBiasingSimulationState>();
            Variables = new TwoPort<double>(state, context);
            int posNode = state.Map[Variables.Right.Positive];
            int negNode = state.Map[Variables.Right.Negative];
            int contPosNode = state.Map[Variables.Left.Positive];
            int contNegNode = state.Map[Variables.Left.Negative];
            Branch = state.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);
            int branchEq = state.Map[Branch];

            BiasingElements = new ElementSet<double>(state.Solver, new[] {
                        new MatrixLocation(posNode, branchEq),
                        new MatrixLocation(negNode, branchEq),
                        new MatrixLocation(branchEq, posNode),
                        new MatrixLocation(branchEq, negNode),
                        new MatrixLocation(branchEq, contPosNode),
                        new MatrixLocation(branchEq, contNegNode)
                    });
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            BiasingElements.Add(1, -1, 1, -1, -1, 1);
        }
    }
}