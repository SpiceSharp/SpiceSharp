using NSubstitute;
using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpTest.Models
{
    public static class ComponentHelper
    {
        public static T ModelTemperature<T, P, B>(this T context, P parameters, Func<IBindingContext, B> factory)
            where T : IComponentBindingContext
            where P : IParameterSet
            where B : ITemperatureBehavior
        {
            var modelContext = Substitute.For<IBindingContext>()
                .Parameter(parameters);

            // Add the same temperature state if possible
            if (context.TryGetState(out ITemperatureSimulationState temperature))
            {
                modelContext.GetState<ITemperatureSimulationState>().Returns(temperature);
                modelContext.TryGetState(out Arg.Any<ITemperatureSimulationState>()).Returns(x => { x[0] = temperature; return true; });
            }

            // Some models also need the biasing simulation parameters
            if (context.TryGetSimulationParameterSet(out BiasingParameters bp))
            {
                modelContext.GetSimulationParameterSet<BiasingParameters>().Returns(bp);
                modelContext.TryGetSimulationParameterSet(out Arg.Any<BiasingParameters>()).Returns(x => { x[0] = bp; return true; });
            }

            // Create the behavior and run it
            var behavior = factory(modelContext);
            ((ITemperatureBehavior)behavior).Temperature();

            // Fill the context with information
            context.ModelBehaviors.GetValue<ITemperatureBehavior>().Returns(behavior);
            context.ModelBehaviors.TryGetValue(typeof(ITemperatureBehavior), out Arg.Any<IBehavior>()).Returns(x => { x[0] = behavior; return true; });
            context.ModelBehaviors.TryGetValue(out Arg.Any<ITemperatureBehavior>()).Returns(x => { x[0] = behavior; return true; });

            context.ModelBehaviors.GetValue<B>().Returns(behavior);
            context.ModelBehaviors.TryGetValue(typeof(B), out Arg.Any<IBehavior>()).Returns(x => { x[0] = behavior; return true; });
            context.ModelBehaviors.TryGetValue(out Arg.Any<B>()).Returns(x => { x[0] = behavior; return true; });

            context.ModelBehaviors.GetParameterSet<P>().Returns(parameters);
            context.ModelBehaviors.TryGetParameterSet(out Arg.Any<P>()).Returns(x => { x[0] = parameters; return true; });

            return context;
        }
    }
}
