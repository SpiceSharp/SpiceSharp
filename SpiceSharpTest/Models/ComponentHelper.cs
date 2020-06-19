namespace SpiceSharpTest.Models
{
    public static class ComponentHelper
    {
        /*
        public static void DoBias<B>(this B behavior, IBindingContext context) where B : IBiasingBehavior
        {
            // First to temperature analysis if necessary
            if (behavior is ITemperatureBehavior temperature)
                temperature.Temperature();

            var bstate = context.GetState<IBiasingSimulationState>();
            int counter = 0;
            bool quit = false;

            // To find out when we need to stop
            var size = bstate.Solver.Size;
            var iteration = new double[size * (size + 1)];
            for (var i = 0; i < iteration.Length; i++)
                iteration[i] = double.NaN;

            while (!quit)
            {
                bstate.Solver.Reset();
                behavior.Load();
                Assert.GreaterOrEqual(20, counter++);

                // We will keep loading until the solver elements don't change anymore
                quit = true;
                double value, last;
                int index;
                for (var r = 0; r < size; r++)
                {
                    for (var c = 0; c < size; c++)
                    {
                        index = r * (size + 1) + c;
                        value = bstate.Solver.FindElement(r + 1, c + 1)?.Value ?? double.NaN;
                        last = iteration[index];
                        if (!double.IsNaN(value))
                        {
                            if (double.IsNaN(last))
                                quit = false;
                            else if (!iteration[index].Equals(value))
                                quit = false;
                        }
                        iteration[index] = value;
                    }

                    index = r * (size + 1) + size;
                    value = bstate.Solver.FindElement(r + 1)?.Value ?? double.NaN;
                    last = iteration[index];
                    if (!double.IsNaN(value))
                    {
                        if (double.IsNaN(last))
                            quit = false;
                        else if (!iteration[index].Equals(value))
                            quit = false;
                    }
                    iteration[index] = value;
                }
            }
        }
        public static void DoTransient<B>(this B behavior, IBindingContext context) where B : ITimeBehavior
        {
            // First to temperature analysis if necessary
            if (behavior is ITemperatureBehavior temperature)
                temperature.Temperature();

            // Then find the bias point
            behavior.DoBias(context);

            // Then initialize the states
            behavior.InitializeStates();

            var bstate = context.GetState<IBiasingSimulationState>();
            int counter = 0;
            bool quit = false;

            // To find out when we need to stop
            var size = bstate.Solver.Size;
            var iteration = new double[size * (size + 1)];
            for (var i = 0; i < iteration.Length; i++)
                iteration[i] = double.NaN;

            while (!quit)
            {
                bstate.Solver.Reset();
                behavior.Load();
                Assert.GreaterOrEqual(20, counter++);

                // We will keep loading until the solver elements don't change anymore
                quit = true;
                double value, last;
                int index;
                for (var r = 0; r < size; r++)
                {
                    for (var c = 0; c < size; c++)
                    {
                        index = r * (size + 1) + c;
                        value = bstate.Solver.FindElement(r + 1, c + 1)?.Value ?? double.NaN;
                        last = iteration[index];
                        if (!double.IsNaN(value))
                        {
                            if (double.IsNaN(last))
                                quit = false;
                            else if (!iteration[index].Equals(value))
                                quit = false;
                        }
                        iteration[index] = value;
                    }

                    index = r * (size + 1) + size;
                    value = bstate.Solver.FindElement(r + 1)?.Value ?? double.NaN;
                    last = iteration[index];
                    if (!double.IsNaN(value))
                    {
                        if (double.IsNaN(last))
                            quit = false;
                        else if (!iteration[index].Equals(value))
                            quit = false;
                    }
                    iteration[index] = value;
                }
            }
        }
        public static T ModelTemperature<T, P, B>(this T context, P parameters, Func<IBindingContext, B> factory)
            where T : IComponentBindingContext
            where P : IParameterSet
            where B : ITemperatureBehavior
        {
            var modelContext = Substitute.For<IBindingContext>() .Parameter(parameters);

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
        */
    }
}
