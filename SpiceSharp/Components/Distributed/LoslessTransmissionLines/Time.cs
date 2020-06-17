﻿using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Distributed;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.LosslessTransmissionLines
{
    /// <summary>
    /// Transient behavior for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="ITimeBehavior"/>
    public class Time : Biasing,
        IBiasingBehavior,
        ITimeBehavior
    {
        private readonly int _pos1, _neg1, _pos2, _neg2, _br1, _br2;
        private readonly ITimeSimulationState _time;
        private readonly ElementSet<double> _elements;

        /// <summary>
        /// Gets the delayed signals.
        /// </summary>
        /// <value>
        /// The signals.
        /// </value>
        public DelayedSignal Signals { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Time" /> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="context">The context.</param>
        public Time(string name, IComponentBindingContext context)
            : base(name, context)
        {
            _time = context.GetState<ITimeSimulationState>();
            _pos1 = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[0])];
            _neg1 = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[1])];
            _pos2 = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[2])];
            _neg2 = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[3])];
            _br1 = BiasingState.Map[Branch1];
            _br2 = BiasingState.Map[Branch2];
            _elements = new ElementSet<double>(BiasingState.Solver, null, new[] { _br1, _br2 });
            Signals = new DelayedSignal(2, Parameters.Delay);
        }

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
            var sol = BiasingState.Solution;

            // Calculate the inputs
            var input1 = sol[_pos2] - sol[_neg2] + Parameters.Impedance * sol[_br2];
            var input2 = sol[_pos1] - sol[_neg1] + Parameters.Impedance * sol[_br1];
            Signals.SetProbedValues(input1, input2);
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            var y = Parameters.Admittance;
            if (_time.UseDc)
            {
                BiasingElements.Add(
                    y, -y, -y, y, 1, 0, -1, -1,
                    y, -y, -y, y, 1, 0, -1, 0,
                    1, -1, 1, 1, 1
                    );
            }
            else
            {
                BiasingElements.Add(
                    y, -y, -y, y, 1, 1, -1, -1,
                    y, -y, -y, y, 1, 1, -1, -1
                    );
            }

            var sol = BiasingState.Solution;

            // Calculate inputs
            var input1 = sol[_pos2] - sol[_neg2] + Parameters.Impedance * sol[_br2];
            var input2 = sol[_pos1] - sol[_neg1] + Parameters.Impedance * sol[_br1];
            Signals.SetProbedValues(input1, input2);

            // Update the branch equations
            _elements.Add(Signals.Values[0], Signals.Values[1]);
        }
    }
}