# Temperature-dependent behaviors

Temperature-dependent behaviors implement the **[ITemperatureBehavior](xref:SpiceSharp.Behaviors.ITemperatureBehavior)** interface. It should contain any calculations that - as the name implies - are temperature-dependent.

This behavior can also be used to calculate properties that would otherwise be done inside much more frequently executed methods. Any expensive calculations that are not necessary inside other behaviors can be added to here.

## Example - A diode

How would we create a temperature-dependent behavior for our diode model?

<p align="center"><img src="images/example_circuit_mna_dio.svg" alt="Diode definition" width="100px" /></p>

$$i_D = I_{SS}\left(e^\frac{v_A-v_B}{\eta V_T}\right)$$

We can see in the denominator that the multiplication of $\eta$ and $V_T = \frac{k\cdot T}{q}$ can be computed in the temperature-dependent behavior, because it is temperature-dependent and doesn't need to be re-evaluated every time the current/voltage changes.

[!code-csharp[Circuit](../../../SpiceSharpTest/Examples/SimpleDiode/DiodeTemperature.cs)]

The first thing to look at is the constructor. It accepts an *[IBindingContext](xref:SpiceSharp.Entities.IBindingContext)*. This interface provides access to:

- The entity or component parameters. In this case, we extract the diode parameters that we created ourselves. This method will throw an exception if the parameters don't exist.
- The simulation states. In this case we are interested in the simulation state that deals with temperature, meaning the *[ITemperatureSimulationState](xref:SpiceSharp.Simulations.ITemperatureSimulationState)*.

This temperature-dependent behaviors can serve as a base for other behaviors that can implement it, so I decided to have them protected. The other behaviors don't really need to deal with temperature, so I kept the simulation state private.

The only method in the class is fairly straight-forward. It is and should be called whenever the temperature changes, and it simply calculates the denominator of our diode equation.