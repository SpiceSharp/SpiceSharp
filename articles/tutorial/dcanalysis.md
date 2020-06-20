# DC analysis

A DC analysis makes it possible to sweep over one or more independent sources, assuming the circuit is static (not changing in time, anywhere in the circuit). You can for example construct the input-output relation of an amplifier using this type of analysis, or find out how a resistor influences other parts of the circuit. It is run using the **[DC](xref:SpiceSharp.Simulations.DC)** class.

Let's consider the following circuit:

<p align="center"><img src="images/example_DC.svg" /></p>

We wish to find the I-V curve for multiple Vgs voltages. This can be achieved by passing multiple **[SweepConfiguration](xref:SpiceSharp.Simulations.SweepConfiguration)**-objects to the **[DC](xref:SpiceSharp.Simulations.DC)** constructor.

The code will look like this:

[!code-csharp[Circuit](../../SpiceSharpTest/BasicExampleTests.cs#example_DC)]

First we build our NMOS transistor *M1*. For this we need an **[IComponent](xref:SpiceSharp.Components.IComponent)** implementation for a mosfet, which we can connect to nodes in the circuit. Most components also need a reference to a model (another **[IEntity](xref:SpiceSharp.Entities.IEntity)** in the same circuit) to function properly. Models typically describe general common properties (eg. threshold voltage, transconducance gain, etc.) while components will typically describe behavior on a device-by-device basis (eg. transistor width and length, device temperature, etc.).

In our case, Spice# provides us with a component that implements the model equations of a transistor, called **[Mosfet1](xref:SpiceSharp.Components.Mosfet1)**, which also needs a model of the type **[Mosfet1Model](xref:SpiceSharp.Components.Mosfet1Model)**. This model is identical to Spice's mosfet LEVEL=1 model.

Every entity can have one or more parameters. The parameter names for MOS level 1 are all specified in the parameter sets **[BaseParameters](xref:SpiceSharp.Components.MosfetBehaviors.Level1.BaseParameters)** and **[ModelBaseParameters](xref:SpiceSharp.Components.MosfetBehaviors.Level1.ModelBaseParameters)**. Remember that these parameter sets extend other classes that also contain parameters! Use the API and the GitHub repository to your advantage to find out which parameters are supported. For most models, the names are identical to their Spice 3f5 counterparts.

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
