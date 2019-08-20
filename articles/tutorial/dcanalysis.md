# DC analysis

This type of analysis makes it possible to sweep over one or more independent sources, assuming the circuit is in steady-state. You can for example construct DC transfer curves of amplifiers using this type of analysis. It is run using the **[DC](xref:SpiceSharp.Simulations.DC)** class.

Let's consider the following circuit:

<p align="center"><img src="images/example_DC.svg" /></p>

We wish to find the I-V curve for multiple Vgs voltages. This can be achieved by passing multiple **[SweepConfiguration](xref:SpiceSharp.Simulations.SweepConfiguration)**-objects to the **[DC](xref:SpiceSharp.Simulations.DC)** constructor.

The code looks as follows:

[!code-csharp[Circuit](../../SpiceSharpTest/BasicExampleTests.cs#example_DC)]

First we build our NMOS transistor *M1*. For this we need a **[Component](xref:SpiceSharp.Components.Component)** implementation of a transistor, which we can connect to nodes in the circuit. Most components also need a **[Model](xref:SpiceSharp.Components.Model)** in order to work. Models typically describe general common properties (eg. threshold voltage, transconducance gain, etc.) while components will typically describe behavior on an individual level (eg. width, length, device temperature, etc.).

In our case, Spice# provides us with a component that implements the model equations of a transistor, called **[Mosfet1](xref:SpiceSharp.Components.Mosfet1)**, which is accompanied by a **[Mosfet1Model](xref:SpiceSharp.Components.Mosfet1Model)**. This model is identical to Spice's mosfet LEVEL=1 model.
Every component or model can have parameters, which can be set by using the method *[SetParameter](xref:SpiceSharp.Circuits.Entity.SetParameter(System.String,System.Collections.Generic.IEqualityComparer{System.String}))*. The parameter names for MOS level 1 are all specified in the parameter sets **[BaseParameters](xref:SpiceSharp.Components.MosfetBehaviors.Level1.BaseParameters)** and **[ModelBaseParameters](xref:SpiceSharp.Components.MosfetBehaviors.Level1.ModelBaseParameters)**. Remember that these parameter sets extend other classes that also contain parameters! Use the API and the GitHub repository to your advantage to find out which parameters are supported. For most models, the names are identical to their Spice 3f5 counterparts.

After running and plotting the data we get:

<p align="center"><img src="images/example_DCgraph.svg" /></p>

If we wanted to implement the same simulation in the original Spice simulator, we would provide the following netlist:

```
NMOS biasing example

.MODEL example NMOS(Kp=150m)
M1 d g 0 0
Vgs g 0 0
Vds d 0 0

.DC Vds 0 5 0.1 Vgs 0 3 0.2

* Export voltages/currents/...

.END
```

This netlist would be parsed, executed, and the results are then written to a file which can then be processed. Spice# is a *library*, and you have access to the data during execution. For example, you could change parts of the simulation during runtime, you could automate designs, and you can freely choose during the simulation which data you want to use and ignore.

<div class="pull-left">[Previous: Analysis](analysis.md)</div> <div class="pull-right">[Next: AC analysis](acanalysis.md)</p>
