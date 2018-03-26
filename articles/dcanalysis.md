# DC analysis

It is possible to sweep over multiple sources for a DC analysis. It is possible to construct DC transfer curves using this type of analysis. This type of simulation is run by the **[DC](xref:SpiceSharp.Simulations.DC)** class. Consider the following circuit:

<p align="center"><img src="images/example_DC.svg" /></p>

We wish to find the I-V curve for multiple Vgs voltages. This can be achieved by passing multiple **[SweepConfiguration](xref:SpiceSharp.Simulations.SweepConfiguration)**-objects to the **[DC](xref:SpiceSharp.Simulations.DC)** constructor.

The code looks as follows:

[!code-csharp[Circuit](../SpiceSharpTest/BasicExampleTests.cs#example_DC)]

First we build an NMOS transistor. For this we need a **[Component](xref:SpiceSharp.Components.Component)**, which we can connect to nodes in the circuit. Most components also need a **[Model](xref:SpiceSharp.Components.Model)** in order to work. Models typically describe common environmental properties (eg. threshold voltage, transconducance gain, etc.) while components will typically describe behavior on an individual level (eg. width, length, device temperature, etc.).

In our case, the component is a  **[Mosfet1](xref:SpiceSharp.Components.Mosfet1)**, which needs a **[Mosfet1Model](xref:SpiceSharp.Components.Mosfet1Model)** to work.
Every component or model can have parameters, which can be set by using the method *[SetParameter](xref:SpiceSharp.Circuits.Entity.SetParameter(System.String,System.Double))*. The parameter names for MOS level 1 transistors are defined in the model specification and are all lower case.

After running and plotting the data we get:

<p align="center"><img src="images/example_DCgraph.svg" /></p>

<div class="pull-left">[Previous: Analysis](analysis.md)</div> <div class="pull-right">[Next: AC analysis](acanalysis.md)</p>