﻿using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.LosslessTransmissionLines
{
    /// <summary>
    /// Load behavior for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="Parameters"/>
    public class Biasing : Behavior,
        IBiasingBehavior,
        IParameterized<Parameters>
    {
        private readonly int _pos1, _neg1, _pos2, _neg2, _int1, _int2, _br1, _br2;

        /// <summary>
        /// Gets the biasing elements.
        /// </summary>
        /// <value>
        /// The biasing elements.
        /// </value>
        protected ElementSet<double> BiasingElements { get; }

        /// <inheritdoc/>
        public Parameters Parameters { get; }

        /// <summary>
        /// Gets the state of the biasing.
        /// </summary>
        /// <value>
        /// The state of the biasing.
        /// </value>
        protected IBiasingSimulationState BiasingState { get; }

        /// <summary>
        /// Gets the left-side internal node.
        /// </summary>
        /// <value>
        /// The left internal node.
        /// </value>
        protected IVariable<double> Internal1 { get; }

        /// <summary>
        /// Gets the right-side internal node.
        /// </summary>
        /// <value>
        /// The right internal node.
        /// </value>
        protected IVariable<double> Internal2 { get; }

        /// <summary>
        /// Gets the left-side branch.
        /// </summary>
        /// <value>
        /// The left branch.
        /// </value>
        protected IVariable<double> Branch1 { get; }

        /// <summary>
        /// Gets the right-side branch.
        /// </summary>
        /// <value>
        /// The right branch.
        /// </value>
        protected IVariable<double> Branch2 { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public Biasing(string name, ComponentBindingContext context)
            : base(name)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(4);

            // Get parameters
            Parameters = context.GetParameterSet<Parameters>();
            BiasingState = context.GetState<IBiasingSimulationState>();
            _pos1 = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[0])];
            _neg1 = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[1])];
            _pos2 = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[2])];
            _neg2 = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[3])];

            Internal1 = BiasingState.CreatePrivateVariable(Name.Combine("int1"), Units.Volt);
            _int1 = BiasingState.Map[Internal1];
            Internal2 = BiasingState.CreatePrivateVariable(Name.Combine("int2"), Units.Volt);
            _int2 = BiasingState.Map[Internal2];
            Branch1 = BiasingState.CreatePrivateVariable(Name.Combine("branch1"), Units.Ampere);
            _br1 = BiasingState.Map[Branch1];
            Branch2 = BiasingState.CreatePrivateVariable(Name.Combine("branch2"), Units.Ampere);
            _br2 = BiasingState.Map[Branch2];

            BiasingElements = new ElementSet<double>(BiasingState.Solver,
                new MatrixLocation(_pos1, _pos1),
                new MatrixLocation(_pos1, _int1),
                new MatrixLocation(_int1, _pos1),
                new MatrixLocation(_int1, _int1),
                new MatrixLocation(_int1, _br1),
                new MatrixLocation(_br1, _int1),
                new MatrixLocation(_neg1, _br1),
                new MatrixLocation(_br1, _neg1),
                new MatrixLocation(_pos2, _pos2),
                new MatrixLocation(_pos2, _int2),
                new MatrixLocation(_int2, _pos2),
                new MatrixLocation(_int2, _int2),
                new MatrixLocation(_int2, _br2),
                new MatrixLocation(_br2, _int2),
                new MatrixLocation(_neg2, _br2),
                new MatrixLocation(_br2, _neg2),

                // These are only used to calculate the biasing point
                new MatrixLocation(_br1, _pos1),
                new MatrixLocation(_br1, _pos2),
                new MatrixLocation(_br1, _neg2),
                new MatrixLocation(_br2, _br1),
                new MatrixLocation(_br2, _br2));
        }

        void IBiasingBehavior.Load()
        {
            var y = Parameters.Admittance;
            BiasingElements.Add(
                y, -y, -y, y, 1, 0, -1, -1,
                y, -y, -y, y, 1, 0, -1, 0,
                1, -1, 1, 1, 1
                );
        }
    }
}