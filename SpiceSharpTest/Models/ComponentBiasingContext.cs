using NSubstitute;
using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Components;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharpTest.Models
{
    public static class ComponentBindingContext
    {
        public static T Nodes<T>(this T context, params string[] nodeNames) where T : IComponentBindingContext
        {
            var variables = new Variable[nodeNames.Length];
            for (var i = 0; i < variables.Length; i++)
                variables[i] = new Variable(nodeNames[i], VariableType.Voltage);

            for (var i = 0; i < nodeNames.Length; i++)
            {
                context.Variables.MapNode(nodeNames[i], VariableType.Voltage).Returns(variables[i]);
                context.Variables.ContainsNode(nodeNames[i]).Returns(true);
                context.Variables.TryGetNode(nodeNames[i], out Arg.Any<Variable>()).Returns(x => { x[0] = variables[i]; return true; });
                context.Nodes[i].Returns(variables[i]);
            }
            context.Nodes.Count.Returns(nodeNames.Length);
            return context;
        }
        public static T Bias<T>(this T context, Action<IBiasingSimulationState> changeBiasing = null, Action<IIterationSimulationState> changeIteration = null) where T : IComponentBindingContext
        {
            // Biasing simulation state
            var state = Substitute.For<IBiasingSimulationState>();
            state.Solver.Returns(LUHelper.CreateSparseRealSolver());
            var solution = new DenseVector<double>(context.Nodes.Count);
            state.Solution.Returns(solution);
            var count = context.Nodes.Count;
            for (var i = 0; i < count; i++)
                state.Map[context.Nodes[i]].Returns(i + 1);
            state.Map.Count.Returns(count);

            changeBiasing?.Invoke(state);
            context.GetState<IBiasingSimulationState>().Returns(state);
            context.TryGetState(out Arg.Any<IBiasingSimulationState>()).Returns(x => { x[0] = state; return true; });

            // Iteration simulation state
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
        public static T Bias<T>(this T context, params Variable[] extra) where T : IComponentBindingContext
        {
            return Bias(context, biasing =>
            {
                var count = biasing.Map.Count;
                for (var i = 0; i < extra.Length; i++)
                    biasing.Map[extra[i]].Returns(count + i + 1);
                biasing.Map.Count.Returns(count + extra.Length);
            });
        }
        public static T Frequency<T>(this T context, Action<IComplexSimulationState> changeComplex = null) where T : IComponentBindingContext
        {
            var state = Substitute.For<IComplexSimulationState>();
            state.Solver.Returns(LUHelper.CreateSparseComplexSolver());
            var solution = new DenseVector<Complex>(context.Nodes.Count);
            state.Solution.Returns(solution);
            var count = context.Nodes.Count;
            for (var i = 0; i < count; i++)
                state.Map[context.Nodes[i]].Returns(i + 1);
            state.Map.Count.Returns(count);
            state.Laplace.Returns(0.0);

            changeComplex?.Invoke(state);
            context.GetState<IComplexSimulationState>().Returns(state);
            context.TryGetState(out Arg.Any<IComplexSimulationState>()).Returns(x => { x[0] = state; return true; });
            return context;
        }
        public static T Frequency<T>(this T context, params Variable[] extra) where T : IComponentBindingContext
        {
            return context.Frequency(cplx =>
            {
                var count = cplx.Map.Count;
                for (var i = 0; i < extra.Length; i++)
                    cplx.Map[extra[i]].Returns(count + i + 1);
                cplx.Map.Count.Returns(count + extra.Length);
            });
        }
        public static T Frequency<T>(this T context, double frequency, Action<IComplexSimulationState> changeComplex = null) where T : IComponentBindingContext
        {
            return context.Frequency(cplx =>
            {
                changeComplex?.Invoke(cplx);
                cplx.Laplace.Returns(new Complex(0, frequency * 2 * Math.PI));
            });
        }
        public static T Transient<T>(this T context, Action<IIntegrationMethod> changeIntegration = null, Action<ITimeSimulationState> changeTime = null) where T : IBindingContext
        {
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
        public static T Transient<T>(this T context, double time, double dt, Action<IIntegrationMethod> changeIntegration = null, Action<ITimeSimulationState> changeTime = null) where T : IBindingContext
        {
            return context.Transient(method =>
            {
                changeIntegration?.Invoke(method);
                method.Time.Returns(time);
                method.BaseTime.Returns(time - dt);
                method.Slope.Returns(1.0 / dt);
            }, changeTime);
        }
        public static T Noise<T>(this T context, Action<INoiseSimulationState> changeNoise = null) where T : IBindingContext
        {
            // Create the noise simulation state
            var state = Substitute.For<INoiseSimulationState>();
            state.InputNoise.Returns(0.0);
            state.OutputNoise.Returns(0.0);
            state.OutputNoiseDensity.Returns(0.0);

            changeNoise?.Invoke(state);
            context.GetState<INoiseSimulationState>().Returns(state);
            context.TryGetState(out Arg.Any<INoiseSimulationState>()).Returns(x => { x[0] = state; return true; });
            return context;
        }
        public static T Parameter<T, P>(this T context, P parameters, Action<P> changeParameters = null) where T : IBindingContext where P : IParameterSet
        {
            changeParameters?.Invoke(parameters);
            parameters.CalculateDefaults();
            context.GetParameterSet<P>().Returns(parameters);
            context.TryGetParameterSet(out Arg.Any<P>()).Returns(x => { x[0] = parameters; return true; });
            return context;
        }
        public static T CreateVariable<T>(this T context, Variable variable, params Variable[] variables) where T : IBindingContext
        {
            context.Variables.Create(Arg.Any<string>(), VariableType.Current).Returns(variable, variables);
            return context;
        }
        public static T BranchControlled<T>(this T context, Variable variable) where T : ICurrentControlledBindingContext
        {
            var behavior = Substitute.For<IBranchedBehavior>();
            behavior.Branch.Returns(variable);
            context.ControlBehaviors.GetValue<IBranchedBehavior>().Returns(behavior);
            return context;
        }

        /// <summary>
        /// Turns the object into a proxy.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The proxy.</returns>
        public static Proxy<T> AsProxy<T>(this T value) => new Proxy<T>(value);
    }
}
