using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpTest.Simulations
{
    /// <summary>
    /// A class for testing a derivative for any integration method.
    /// </summary>
    public class IntegralTester : Entity
    {
        private readonly Func<double, double> _reference;
        private readonly double _initial, _relTol, _absTol;
        public IntegralTester(Func<double, double> reference, double initial, double relTol, double absTol)
            : base("dF/dx")
        {
            _initial = initial;
            _reference = reference;
            _relTol = relTol;
            _absTol = absTol;
        }

        public override IEntity Clone()
            => (IEntity)MemberwiseClone();

        public override void CreateBehaviors(ISimulation simulation)
        {
            var eb = new BehaviorContainer("dF/dx");
            var context = new BindingContext(this, simulation, eb);
            eb.Add(new IntegralTesterBehavior(context, _reference, _initial, _relTol, _absTol));
            simulation.EntityBehaviors.Add(eb);
        }
        public class IntegralTesterBehavior : Behavior, IBiasingBehavior, ITimeBehavior, IAcceptBehavior
        {
            private readonly IBiasingSimulationState _biasing;
            private readonly IIntegrationMethod _method;
            private readonly ITimeSimulationState _time;
            private readonly IIntegral _integral;
            private readonly Func<double, double> _reference;
            private readonly double _initial, _relTol, _absTol;
            public IntegralTesterBehavior(IBindingContext context, Func<double, double> reference, double initial, double relTol, double absTol)
                : base(context)
            {
                _biasing = context.GetState<IBiasingSimulationState>();
                _method = context.GetState<IIntegrationMethod>();
                _time = context.GetState<ITimeSimulationState>();
                _integral = _method.CreateIntegral();
                _reference = reference;
                _initial = initial;
                _relTol = relTol;
                _absTol = absTol;
            }

            void ITimeBehavior.InitializeStates()
            {
                _integral.Derivative = _initial;
            }
            void IBiasingBehavior.Load()
            {
                if (_time.UseDc || _method.Time.Equals(0.0))
                    return;
                _integral.Derivative = _biasing.Solution[1];
                _integral.Integrate();
            }
            void IAcceptBehavior.Probe()
            {
            }
            void IAcceptBehavior.Accept()
            {
                if (_method.Time > 0)
                    Helper.AreEqual(_reference(_method.Time), _integral.Value, _relTol, _absTol);
            }
        }
    }
}
