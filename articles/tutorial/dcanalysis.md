# DC analysis

This type of analysis makes it possible to sweep over one or more independent sources, assuming the circuit is in steady-state. You can for example construct DC transfer curves of amplifiers using this type of analysis. It is run using the **[DC](xref:SpiceSharp.Simulations.DC)** class.

Let's consider the following circuit:

<p align="center"><img src="images/example_DC.svg" /></p>

We wish to find the I-V curve for multiple Vgs voltages. This can be achieved by passing multiple **[SweepConfiguration](xref:SpiceSharp.Simulations.SweepConfiguration)**-objects to the **[DC](xref:SpiceSharp.Simulations.DC)** constructor.

The code looks as follows:

[!code-csharp[Circuit](../../SpiceSharpTest/BasicExampleTests.cs#example_DC)]

First we will build an NMOS transistor. For this we need a **[Component](xref:SpiceSharp.Components.Component)** implementation of a transistor, which we can connect to nodes in the circuit. Most components also need a **[Model](xref:SpiceSharp.Components.Model)** in order to work. Models typically describe general common properties (eg. threshold voltage, transconducance gain, etc.) while components will typically describe behavior on an individual level (eg. width, length, device temperature, etc.).

In our case, Spice# provides us with a component that implements the model equations of a transistor, called **[Mosfet1](xref:SpiceSharp.Components.Mosfet1)**, which is accompanied by a **[Mosfet1Model](xref:SpiceSharp.Components.Mosfet1Model)**.
Every component or model can have parameters, which can be set by using the method *[SetParameter](xref:SpiceSharp.Circuits.Entity.SetParameter(System.String,System.Double))*. The parameter names for MOS level 1 transistors are defined in the model specification and are all lower case.

After running and plotting the data we get:

<p align="center"><img src="images/example_DCgraph.svg" /></p>

Running this example in Spice 3f5 would involve writing the following netlist.

```
.MODEL example NMOS(Kp=150m)
M1 d g 0 0
Vgs g 0 0
Vds d 0 0

.DC Vds 0 5 0.1 Vgs 0 3 0.2
```

<div class="pull-left">[Previous: Analysis](analysis.md)</div> <div class="pull-right">[Next: AC analysis](acanalysis.md)</p>
