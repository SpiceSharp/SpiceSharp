# AC analysis

An AC analysis allows you to find the behavior of a circuit for small signals. The simulation will first determine the operating point, after which small sinusoidal excitations are applied on top of the calculated biasing conditions. Each voltage, current or other simulated quantity will then vary along with the excited signal and will show an amplitude and phase shift that is represented by a single complex number. This is useful when characterizing a filter, or when analyzing stability of systems that have feedback in them.

By assuming the perturbations are small and sinusoidal, the circuit can be linearized which makes this type of simulation relatively fast. The result is a *Complex* number, which contains the amplitude and phase information.

Consider the following circuit:

<p align="center"><img src="images/example_AC.svg" /></p>

To simulate this, we can write

[!code-csharp[Circuit](../../SpiceSharpTest/BasicExampleTests.cs#example_AC)]

For our AC analysis, we need an AC source that will excite the circuit. The amplitude and phase of the excitation can be set by using the *acmag* and *acphase* parameters for a **[VoltageSource](xref:SpiceSharp.Components.VoltageSource)** object.

The frequency points that are simulated in our example range from 10mHz to 1kHz, simulating 5 points per decade, logarithmically spaced.

This is effectively the same as using the following netlist in other Spice simulators:

```
AC example

V1 in 0 0 AC 1
R1 in out 10k
C1 out 0 1u

.AC dec 5 10m 1k

* Export voltages/currents/etc.

.END
```

Plotting the output amplitude in decibels gives the following low-pass filter characteristic:

<p align="center"><img src="images/example_ACgraph.svg" /></p>

<div class="pull-left">[Previous: DC analysis](dcanalysis.md)</div> <div class="pull-right">[Next: Transient analysis](transientanalysis.md)</div>
