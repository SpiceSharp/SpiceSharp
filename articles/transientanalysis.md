# Transient analysis

A transient analysis will attempt to include as many effects possible. The unfortunate consequence is that this type of simulation is also by far the slowest. Luckily, it is also relatively straightforward to understand.

Let's use our RC filter from before and apply a pulsed voltage source.

<p align="center"><img src="images/example_AC.svg" /></p>

Associated code:

[!code-csharp[Circuit](../SpiceSharpTest/BasicExampleTests.cs#example_Transient)]

The voltage source now is passed a **[Pulse](xref:SpiceSharp.Components.Pulse)** object that will calculate the voltage in time for us.

The **[Transient](xref:SpiceSharp.Simulations.Transient)** simulation expects a *timestep* that will be used to calculate the initial timestep for the simulation, and a *final time* that tells the analysis the last time point to simulate.

The resulting waveforms look as follows:

<p align="center"><img src="images/example_TransientGraph.svg" /></p>

<div class="pull-left">[Previous: AC anslysis](acanalysis.md)</div> <div class="pull-right">Next</p>