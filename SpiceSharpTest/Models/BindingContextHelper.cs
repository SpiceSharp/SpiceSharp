using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models
{
    public static class BindingContextHelper
    {
        private class TmpDerivative : IDerivative
        {
            private readonly double _dt;
            public double Value { get; set; }
            public double Derivative => 1.0;
            public TmpDerivative(double dt)
            {
                _dt = dt;
            }
            public double GetPreviousValue(int index) => 0.0;
            public double GetPreviousDerivative(int index) => 1.0;
            public JacobianInfo GetContributions(double coefficient, double currentValue)
            {
                double g = coefficient / _dt;
                return new JacobianInfo(g, Derivative - g * currentValue);
            }
            public JacobianInfo GetContributions(double coefficient)
            {
                double h = 1.0 / _dt;
                return new JacobianInfo(
                    h * coefficient,
                    Derivative - h * Value);
            }
            public void Derive()
            {
            }
        }

        /*
        /// <summary>
        /// The context should return the following variables.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="variables">The variables.</param>
        /// <returns></returns>
        public static T Variable<T>(this T context, params IVariable<double>[] variables) where T : IBindingContext
        {
            if (variables == null || variables.Length == 0)
                return context;

            var newVariables = new IVariable[count + variables.Length];
            int index = 0;
            foreach (var v in context.Variables)
                newVariables[index++] = v;

            for (var i = 0; i < variables.Length; i++)
            {
                context.Variables.Contains(variables[i].Name).Returns(true);
                context.Variables[variables[i].Name].Returns(variables[i]);
                newVariables[index] = variables[i];
                if (variables[i].Unit == Units.Volt)
                {
                    context.Variables.MapNode(variables[i].Name, Units.Volt).Returns(variables[i]);
                    context.Variables.TryGetNode(variables[i].Name, out Arg.Any<IVariable>()).Returns(x => { x[0] = variables[i]; return true; });
                    context.Variables.ContainsNode(variables[i].Name).Returns(true);
                }
                index++;
                count++;
            }
            context.Variables.Count.Returns(count);
            context.Variables.GetEnumerator().Returns(_ => ((IEnumerable<IVariable>)newVariables).GetEnumerator());
            return context;
        }
        public static T Nodes<T>(this T context, params string[] nodeNames) where T : IComponentBindingContext
        {
            // Create our variables
            var variables = new IVariable[nodeNames.Length];
            for (var i = 0; i < variables.Length; i++)
            {
                variables[i] = new Variable(nodeNames[i], Units.Volt);
                context.Nodes[i].Returns(variables[i]);
            }
            context.Nodes.Count.Returns(variables.Length);
            context.Variable(variables);
            return context;
        }
        public static T CreateVariable<T>(this T context, params IVariable[] variables) where T : IBindingContext
        {
            context.Variable(variables);

            // Make sure the context returns these variables when a new one is requested
            var next = new IVariable[variables.Length - 1];
            for (var i = 1; i < next.Length; i++)
                next[i - 1] = variables[i];
            context.Variables.Create(Arg.Any<string>(), Arg.Any<IUnit>()).Returns(variables[0], next);
            return context;
        }
        public static T BranchControlled<T>(this T context, IVariable<double> variable) where T : ICurrentControlledBindingContext
        {
            context.Variable(variable);
            var behavior = Substitute.For<IBranchedBehavior<double>>();
            behavior.Branch.Returns(variable);
            context.ControlBehaviors.GetValue<IBranchedBehavior<double>>().Returns(behavior);
            return context;
        }
        public static T BranchControlled<T>(this T context, IVariable<Complex> variable) where T : ICurrentControlledBindingContext
        {
            context.Variable(variable);
            var behavior = Substitute.For<IBranchedBehavior<Complex>>();
            behavior.Branch.Returns(variable);
            context.ControlBehaviors.GetValue<IBranchedBehavior<Complex>>().Returns(behavior);
            return context;
        }
        public static T Parameter<T, P>(this T context, P parameters) where T : IBindingContext where P : IParameterSet
        {
            // Make sure the defaults are calculated
            parameters.CalculateDefaults();

            // Make the context return these parameters
            context.GetParameterSet<P>().Returns(parameters);
            context.TryGetParameterSet(out Arg.Any<P>()).Returns(x => { x[0] = parameters; return true; });
            return context;
        }
        public static T Temperature<T>(this T context, 
            double temperature = Constants.ReferenceTemperature, 
            double nominalTemperature = Constants.ReferenceTemperature) where T : IBindingContext
        {
            // An ITemperatureSimulation has an ITemperatureSimulationState
            var state = Substitute.For<ITemperatureSimulationState>();
            state.Temperature.Returns(temperature);
            state.NominalTemperature.Returns(nominalTemperature);

            context.GetState<ITemperatureSimulationState>().Returns(state);
            context.TryGetState(out Arg.Any<ITemperatureSimulationState>()).Returns(x => { x[0] = state; return true; });
            return context;
        }
        public static T Bias<T>(this T context, Action<IBiasingSimulationState> changeBiasing = null, Action<IIterationSimulationState> changeIteration = null) where T : IBindingContext
        {
            // An IBiasingSimulation implements ITemperatureSimulation
            if (!context.TryGetState(out ITemperatureSimulationState _))
                context.Temperature();

            // A BiasingSimulation defines BiasingParameters
            context.SimulationParameter(new BiasingParameters());

            // An IBiasingSimulation has an IBiasingSimulationState
            var state = Substitute.For<IBiasingSimulationState>();
            state.Solver.Returns(LUHelper.CreateSparseRealSolver());
            var solution = new DenseVector<double>(context.Variables.Count);
            state.Solution.Returns(solution);
            int index = 1, count = context.Variables.Count;
            foreach (var v in context.Variables)
                state.Map[v].Returns(index++);
            state.Map.Count.Returns(count);

            changeBiasing?.Invoke(state);
            context.GetState<IBiasingSimulationState>().Returns(state);
            context.TryGetState(out Arg.Any<IBiasingSimulationState>()).Returns(x => { x[0] = state; return true; });

            // Not required for an IBiasingSimulation, but the implementation
            // BiasingSimulation has an IIterationSimulationState
            var istate = Substitute.For<IIterationSimulationState>();
            istate.SourceFactor.Returns(1.0);
            istate.Gmin.Returns(1e-20);
            istate.IsConvergent.Returns(true);
            istate.Mode.Returns(IterationModes.Junction);

            changeIteration?.Invoke(istate);
            context.GetState<IIterationSimulationState>().Returns(istate);
            context.TryGetState(out Arg.Any<IIterationSimulationState>()).Returns(x => { x[0] = istate; return true; });

            return context;
        }
        public static T Frequency<T>(this T context, Action<IComplexSimulationState> changeComplex = null) where T : IBindingContext
        {
            // An IFrequencySimulation implements an IBiasingSimulation
            if (!context.TryGetState(out IBiasingSimulationState _))
                context.Bias();

            var state = Substitute.For<IComplexSimulationState>();
            state.Solver.Returns(new SparseComplexSolver());
            var solution = new DenseVector<Complex>(context.Variables.Count);
            state.Solution.Returns(solution);
            state.Laplace.Returns(0.0);
            int index = 1, count = context.Variables.Count;
            foreach (var v in context.Variables)
                state.Map[v].Returns(index++);
            state.Map.Count.Returns(count);

            changeComplex?.Invoke(state);
            context.GetState<IComplexSimulationState>().Returns(state);
            context.TryGetState(out Arg.Any<IComplexSimulationState>()).Returns(x => { x[0] = state; return true; });
            return context;
        }
        public static T Frequency<T>(this T context, double frequency) where T : IBindingContext
        {
            return context.Frequency(cplx =>
            {
                cplx.Laplace.Returns(new Complex(0, frequency * 2 * Math.PI));
            });
        }
        public static T Transient<T>(this T context, Action<IIntegrationMethod> changeIntegration = null, Action<ITimeSimulationState> changeTime = null) where T : IBindingContext
        {
            // An ITimeSimulation implements an IBiasingSimulation
            if (!context.TryGetState(out IBiasingSimulationState _))
                context.Bias();

            // Create the time simulation state
            var state = Substitute.For<ITimeSimulationState>();
            state.UseDc.Returns(true);
            state.UseIc.Returns(false);

            changeTime?.Invoke(state);
            context.GetState<ITimeSimulationState>().Returns(state);
            context.TryGetState(out Arg.Any<ITimeSimulationState>()).Returns(x => { x[0] = state; return true; });

            // Create the integration method
            var method = Substitute.For<IIntegrationMethod>();
            method.Time.Returns(0.0);
            method.BaseTime.Returns(0.0);
            method.Slope.Returns(1.0);
            method.Order.Returns(1);
            method.MaxOrder.Returns(2);

            changeIntegration?.Invoke(method);
            context.GetState<IIntegrationMethod>().Returns(method);
            context.TryGetState(out Arg.Any<IIntegrationMethod>()).Returns(x => { x[0] = method; return true; });
            return context;
        }
        public static T Transient<T>(this T context, double time, double dt, Action<IIntegrationMethod> changeIntegration = null, Action<ITimeSimulationState> changeTime = null) where T : IComponentBindingContext
        {
            return context.Transient(method =>
            {
                changeIntegration?.Invoke(method);
                method.Time.Returns(time);
                method.BaseTime.Returns(Math.Max(time - dt, 0.0));
                method.Slope.Returns(1.0 / dt);

                method.CreateDerivative().Returns(callinfo => new TmpDerivative(dt));
            }, state =>
            {
                changeTime?.Invoke(state);
                state.UseDc.Returns(time <= 0.0);
            });
        }
        public static T Noise<T>(this T context, Action<INoiseSimulationState> changeNoise = null) where T : IComponentBindingContext
        {
            // An Noise simulation implements IFrequencySimulation
            if (!context.TryGetState(out IComplexSimulationState _))
                context.Frequency();

            // Create the noise simulation state
            var state = Substitute.For<INoiseSimulationState>();
            state.DeltaFrequency.Returns(1.0);
            state.Frequency.Returns(1.0);
            state.InputNoise.Returns(0.0);
            state.OutputNoise.Returns(0.0);
            state.OutputNoiseDensity.Returns(0.0);

            changeNoise?.Invoke(state);
            context.GetState<INoiseSimulationState>().Returns(state);
            context.TryGetState(out Arg.Any<INoiseSimulationState>()).Returns(x => { x[0] = state; return true; });
            return context;
        }
        public static T ModelBehavior<T, B>(this T context, B behavior) where T : IComponentBindingContext where B : IBehavior
        {
            void Register<TB>(TB behavior) where TB : IBehavior
            {
                context.ModelBehaviors.GetValue<TB>().Returns(behavior);
                context.ModelBehaviors.TryGetValue<TB>(out Arg.Any<TB>()).Returns(x => { x[0] = behavior; return true; });
                context.ModelBehaviors.TryGetValue(typeof(TB), out Arg.Any<IBehavior>()).Returns(x => { x[0] = behavior; return true; });
            }
            Register(behavior);
            if (behavior is ITemperatureBehavior temperature)
                Register(temperature);
            if (behavior is IBiasingBehavior biasing)
                Register(biasing);
            if (behavior is IBiasingUpdateBehavior biasingUpdate)
                Register(biasingUpdate);
            if (behavior is IFrequencyBehavior frequency)
                Register(frequency);
            if (behavior is IFrequencyUpdateBehavior frequencyUpdate)
                Register(frequencyUpdate);
            if (behavior is INoiseBehavior noise)
                Register(noise);
            if (behavior is ITimeBehavior time)
                Register(time);
            if (behavior is IAcceptBehavior accept)
                Register(accept);
            if (behavior is IConvergenceBehavior convergence)
                Register(convergence);
            return context;
        }
        public static T ModelParameters<T, B>(this T context, B modelParameters) where T : IComponentBindingContext where B : IParameterSet
        {
            context.ModelBehaviors.GetParameterSet<B>().Returns(modelParameters);
            context.ModelBehaviors.TryGetParameterSet(out Arg.Any<B>()).Returns(x => { x[0] = modelParameters; return true; });
            return context;
        }
        public static T SimulationParameter<T, P>(this T context, P parameters) where T : IBindingContext where P : IParameterSet
        {
            context.GetSimulationParameterSet<P>().Returns(parameters);
            context.TryGetSimulationParameterSet(out Arg.Any<P>()).Returns(x => { x[0] = parameters; return true; });
            return context;
        }

        /// <summary>
        /// Turns the object into a proxy.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The proxy.</returns>
        public static Proxy<T> AsProxy<T>(this T value) => new Proxy<T>(value);
        */
    }
}
