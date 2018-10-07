# AC analysis

An AC analysis allows you to find the behavior of a circuit for small signals. The simulation will first determine the operating point, after which small perturbations are applied. The result of this analysis indicates how the circuit behaves for these small perturbations. This is useful when the gain and bandwidth needs to be characterized of a filter.

By assuming the perturbations are small and sinusoidal, the circuit can be linearized which makes this type of simulation relatively fast. The result is a *Complex* number, which contain the amplitude and phase information.

Consider the following circuit:

<p align="center"><img src="images/example_AC.svg" /></p>

To simulate this, we can write

[!code-csharp[Circuit](../SpiceSharpTest/BasicExampleTests.cs#example_AC)]

For our AC analysis, we need an AC source that will excite the circuit. The amplitude and phase of the excitation can be set by using the *acmag* and *acphase* parameters for a **[Voltage source](xref:SpiceSharp.Components.VoltageSource)**.

The frequency points that are simulated range from 10mHz to 1kHz, simulating 5 points per decade, logarithmically spaced.

This is effectively the same as using the following netlist in Spice 3f5:

```
V1 in 0 0 AC 1
R1 in out 10k
C1 out 0 1u

.AC dec 5 10m 1k
```

Plotting the output amplitude (dB) yields the following graph:

<p align="center"><img src="images/example_ACgraph.svg" /></p>

<div class="pull-left">[Previous: DC analysis](dcanalysis.md)</div> <div class="pull-right">[Next: Transient analysis](transientanalysis.md)</div>
