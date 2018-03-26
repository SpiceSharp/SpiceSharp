# AC analysis

AC analysis allows you to find the behavior of a circuit for small signals. The circuit is solved for a specific operating point, after which small perturbations are applied. The result of this analysis indicates how the circuit behaves for these small perturbations.
When doing this, the circuit can be significantly simplified. The perturbations are considered to change with a certain amplitude and frequency. The frequency is swept by the analysis.

Consider the following circuit:

<p align="center"><img src="images/example_AC.svg" /></p>

To simulate this, we can write

[!code-csharp[Circuit](../SpiceSharpTest/BasicExampleTests.cs#example_AC)]

For AC analysis we need a source for our perturbations. The amplitude and phase of the excitation can be set by using the *acmag* and *acphase* parameters for **[Voltage source](xref:SpiceSharp.Components.VoltageSource)**.

The frequency points that are simulated range from 10mHz to 1kHz, simulating 5 points per decade, logarithmically spaced.

Plotting the output amplitude (dB) yields the following graph:

<p align="center"><img src="images/example_ACgraph.svg" /></p>

<div class="pull-left">[Previous: DC analysis](dcanalysis.md)</div> <div class="pull-right">[Next: Transient analysis](transientanalysis.md)</p>