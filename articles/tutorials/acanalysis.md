# AC analysis

An AC analysis allows you to find the behavior of a circuit for small signals. The simulation will first determine the operating point, after which small sinusoidal perturbations are applied on top of the calculated biasing conditions. Each voltage, current or other simulated quantity will then vary along with the perturbation and will have its own amplitude and phase shift that is represented by a single complex number. This is useful when characterizing a filter, or when analyzing stability of systems that have feedback.

By assuming that the perturbations are small and sinusoidal, the circuit can be linearized which makes this type of simulation relatively fast. The result is a *Complex* number, which contains the amplitude and phase information.

Consider the following circuit:

<p align="center"><img src="images/example_AC.svg" /></p>

To simulate this, we can write

[!code-csharp[Circuit](../../SpiceSharpTest/BasicExampleTests.cs#example_AC)]

The independent voltage source `V1` will be the one applying the small sinusoidal perturbation, and it will give that perturbation a weight/amplitude of 1. The amplitude and phase of the excitation can be set by using the *acmag* and *acphase* parameters of the **[VoltageSource](xref:SpiceSharp.Components.VoltageSource)** instance. Current sources can also be set to perturb the system using the same kind of parameters.

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

Plotting the output amplitude in decibels gives the following low-pass filter characteristic, neatly showing the bandwidth to be a bit higher than 10Hz (the exact $f_{-3dB} = 15.915Hz$).

<p align="center"><img src="images/example_ACgraph.svg" /></p>
