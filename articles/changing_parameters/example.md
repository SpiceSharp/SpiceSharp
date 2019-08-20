# Example

In this section we will take a look at one way of changing a parameter *during* a
transient simulation. For this, we'll use the following circuit.

<p align="center"><img width="250px" src="images/example_lpf_resdiv.svg" /></p>

[!code-csharp[Circuit](../../SpiceSharpTest/Examples/ChangingParameter/Changing.cs#example_change_parameter_circuit)]

We also create our transient simulation as we normally would.

[!code-csharp[Circuit](../../SpiceSharpTest/Examples/ChangingParameter/Changing.cs#example_change_parameter_transient)]

So far so good. Nothing has really changed from before. We will now subscribe to the necessary events to modify the value of *R2*. We want it to change linearly with time from ![1kOhm](https://latex.codecogs.com/svg.latex?\inline&space;1k\Omega) to ![11kOhm](https://latex.codecogs.com/svg.latex?\inline&space;11k\Omega).

## The biasing behavior of a resistor

The resistance of a resistor can be changed using the **[BaseParameters](xref:SpiceSharp.Components.ResistorBehaviors.BaseParameters)** of that resistor. But we cannot change the base parameters directly from the entity stored in the circuit! In other words, the following will fail to change the resistance for the simulation.

```csharp
ckt.Object["R1"].SetParameter(newResistance);
```

 Any simulation may *clone* all parameters to make it independent of other simulations. So we will need to **ask our simulation** - rather than the entity - for the base parameters.

Another something to consider is that even if we change the resistance parameter, the **[TemperatureBehavior](xref:SpiceSharp.Components.ResistorBehaviors.TemperatureBehavior)** will compute the *conductance* that is loaded by the load behavior of the resistor. So in order to update the resistance of a resistor, we will need to execute the following steps.
1. Change the resistance in the **[BaseParameters](xref:SpiceSharp.Components.ResistorBehaviors.BaseParameters)**, extracted from our **simulation**.
2. Call the *Temperature* method of the **[TemperatureBehavior](xref:SpiceSharp.Components.ResistorBehaviors.TemperatureBehavior)**, also extracted from our **simulation**.

### Requesting the parameters and behaviors

All entity parameters and behaviors are loaded during *setup* of the simulation. So we can use the *AfterSetup* event of the simulation to extract them.

[!code-csharp[Extract parameters and behaviors](../../SpiceSharpTest/Examples/ChangingParameter/Changing.cs#example_change_parameter_setup)]

### Updating the parameters

We need to update the resistance every time the simulation is getting ready to *load* the Y-matrix and RHS-vector. In other words, by registering for the *BeforeLoad* event, we can be sure that the resistance is always updated with the latest value.

[!code-csharp[Change parameter](../../SpiceSharpTest/Examples/ChangingParameter/Changing.cs#example_change_parameter_load)]

Combining all these code snippets finally results in the following simulation output.

<p align="center"><img src="images/example_lpf_resdiv_graph.svg" /></p>

<div class="pull-left">[Previous: Changing parameters during simulation](changing_parameters_during_simulation.md)</div>
