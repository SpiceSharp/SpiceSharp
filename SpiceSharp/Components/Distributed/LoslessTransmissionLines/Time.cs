using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Distributed;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.LosslessTransmissionLines
{
    /// <summary>
    /// Transient behavior for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="ITimeBehavior"/>
    [BehaviorFor(typeof(LosslessTransmissionLine)), AddBehaviorIfNo(typeof(ITimeBehavior))]
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
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Time(IComponentBindingContext context)
            : base(context)
        {
            _time = context.GetState<ITimeSimulationState>();
            _pos1 = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[0])];
            _neg1 = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[1])];
            _pos2 = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[2])];
            _neg2 = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[3])];
            _br1 = BiasingState.Map[Branch1];
            _br2 = BiasingState.Map[Branch2];
            _elements = new ElementSet<double>(BiasingState.Solver, [
                new MatrixLocation(_br1, _pos2),
                new MatrixLocation(_br1, _neg2),
                new MatrixLocation(_br1, _br2),
                new MatrixLocation(_br2, _pos1),
                new MatrixLocation(_br2, _neg1),
                new MatrixLocation(_br2, _br1)
            ], [_br1, _br2]);
            Signals = new DelayedSignal(2, Parameters.Delay);
        }

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
            var sol = BiasingState.Solution;

            // Calculate the inputs
            double z = Parameters.Impedance / Parameters.ParallelMultiplier;
            double input1 = sol[_pos2] - sol[_neg2] + z * sol[_br2];
            double input2 = sol[_pos1] - sol[_neg1] + z * sol[_br1];
            Signals.SetProbedValues(input1, input2);
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            double m = Parameters.ParallelMultiplier;
            double y = Parameters.Admittance * m;
            var sol = BiasingState.Solution;

            // Calculate inputs
            double z = Parameters.Impedance / m;
            double input1 = sol[_pos2] - sol[_neg2] + z * sol[_br2];
            double input2 = sol[_pos1] - sol[_neg1] + z * sol[_br1];
            Signals.SetProbedValues(input1, input2);

            // Apply contributions to the Y-matrix and right-hand side vector
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
            double c = Signals.InputDerivative;
            double d = -c * z;
            _elements.Add(
                -c, c, d,
                -c, c, d,
                Signals.Values[0], Signals.Values[1]);
        }
    }
}