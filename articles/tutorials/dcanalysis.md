# DC analysis

A DC analysis makes it possible to sweep over one or more independent sources, assuming the circuit is static (not changing in time, anywhere in the circuit). You can for example construct the input-output relation of an amplifier using this type of analysis, or find out how a resistor influences other parts of the circuit. It is run using the **[DC](xref:SpiceSharp.Simulations.DC)** class.

Let's consider the following circuit:

<p align="center"><img src="images/example_DC.svg" /></p>

We wish to find the $i_{DS}$-$v_{DS}$ curve for multiple $v_{GS}$ voltages. This can be achieved by passing multiple **[ISweep](xref:SpiceSharp.Simulations.ISweep)** instances to the simulation's constructor (or you can add them in the simulation's parameters).

The code will look like this:

[!code-csharp[Circuit](../../SpiceSharpTest/BasicExampleTests.cs#example_DC)]

First we build our NMOS transistor *M1*. For this we need an **[IComponent](xref:SpiceSharp.Components.IComponent)** implementation for a mosfet, which we can connect to nodes in the circuit. Most components also need a reference to a model (another **[IEntity](xref:SpiceSharp.Entities.IEntity)** in the same circuit) to function properly. Models typically describe general common properties (eg. threshold voltage, transconducance gain, etc.) while components will typically describe behavior on a device-by-device basis (eg. transistor width and length, device temperature, etc.).

In our case, Spice# provides us with a component that implements the model equations of a transistor, called **[Mosfet1](xref:SpiceSharp.Components.Mosfet1)**, which also needs a model of the type **[Mosfet1Model](xref:SpiceSharp.Components.Mosfet1Model)**. This model is identical to Spice's mosfet LEVEL=1 model.

Every entity can have one or more parameters, which our stored in parameter sets. The parameters can be accessed directly, or using their given name in conjunction with the **[SetParameter](xref:SpiceSharp.ParameterSets.IParameterSet#SpiceSharp_ParameterSets_IParameterSet_SetParameter__1_System_String___0_)** method. The mosfet's **[Parameters](xref:SpiceSharp.Components.Mosfets.Parameters)** and **[ModelParameters](xref:SpiceSharp.Components.Mosfets.Level1.ModelParameters)** contain all the available parameters, including their given names which are defined by a **[ParameterName](xref:SpiceSharp.Attributes.ParameterNameAttribute)** attribute. Do keep in mind that parameter sets can extend other parameter sets that also contain parameters! Use the API and the GitHub repository to your advantage to find out which parameters you can change.

After running and plotting the data (plotting is not supported by the core package) we get:

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

This netlist would be parsed, executed, and the results are then written to a file which can then be processed. Spice# is a *library*, which means that you have access to the data during execution, giving you more flexibility on how you want the simulation to be run as the simulation data is coming in.