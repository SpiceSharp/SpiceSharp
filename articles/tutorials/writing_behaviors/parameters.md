# Parameters

In many cases, electronic components are described by *parameters*. Parameters can be set on entities or electronic components, and are passed to the simulation behaviors when they are created.

## Example - A diode

A diode is described by the following conventions:

<p align="center"><img src="images/example_circuit_mna_dio.svg" alt="Diode definition" width="100px" /></p>

$$i_D = I_{SS}\left(e^\frac{v_A-v_B}{\eta V_T}\right)$$

In the equation that describes the diode, we can see that we have two parameters: $I_{SS}$ and $\eta$. The factor $V_T = \frac{k\cdot T}{q}$ is in fact only determined by constants (the Boltzmann constant $k$, electron charge $q$ and temperature $T$).

[!code-csharp[Circuit](../../../SpiceSharpTest/Examples/SimpleDiode/DiodeParameters.cs)]
