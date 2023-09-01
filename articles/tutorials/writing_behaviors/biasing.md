# Biasing behaviors

Biasing behaviors describe the DC operation. That means, no time-dependent or frequency-dependent calculations need to be done here (although they can). This behavior is described by the *[IBiasingBehavior](xref:SpiceSharp.Behaviors.IBiasingBehavior)*.

In order to understand what happens in this behavior, you need to understand what *[Modified Nodal Analysis (MNA)](modified_nodal_analysis.md)* is.

In general, what we need to do is gain access to the Y-matrix (the Jacobian), and the right-hand side vector. Then we need to find out which row represents the KCL equation that we need. Then, we need to calculate the current-contributions to it.

To summarize, we are interested in both the current, and the derivatives of that current to any other variable that needs to be solved (usually the voltages). So if we have a current $i_A$ that flows *into* node A and depends on some voltages $v_1, v_2, ...$, then we need to know

- All partial derivatives of $i_A$ with respect to all the unknown voltages ($\frac{\partial i_A}{\partial v_i}$).
- Of course the actual current (using all those voltage).

While loading, we then need to *load* the Y-matrix as follows:

- $Y_{A,i} = \frac{\partial i_A}{\partial v_i}$
- $rhs_i = i_A - \Sigma_i \frac{\partial i_A}{\partial v_i}\cdot v_i = i_A - \Sigma_i Y_{A,i}\cdot v_i$

This is probably one of the trickiest parts to understand and implement, but it is also the part where all the magic happens!

## Example - A diode

We calculate these derivatives for our diode example.

<p align="center"><img src="images/example_circuit_mna_dio.svg" alt="Diode definition" width="100px" /></p>

$$i_D = I_{SS}\left(e^\frac{v_A-v_B}{\eta V_T}\right)$$

The current depends on two voltage, $v_A$ and $v_B$. So we need to calculate the derivative of $i_D$ with respect to those two voltage. We find:

$$\frac{\partial i_D}{\partial v_A} = -\frac{\partial i_D}{\partial v_B} = \frac{i_D}{\eta \cdot V_T}$$

The actual implementation looks like this:

[!code-csharp[Circuit](../../../SpiceSharpTest/Examples/SimpleDiode/DiodeBiasing.cs)]

Rather than an *[IBindingContext](xref:SpiceSharp.Entities.IBindingContext)*, we now use an *[IComponentBindingContext](xref:SpiceSharp.Components.IComponentBindingContext)*, which also contains information about the nodes our component is connected to!

First, we want to have access to the *[IBiasingSimulationState](xref:SpiceSharp.Simulations.IBiasingSimulationState)*, which keeps track of these node voltages, and also contains the solver used to solve the KCL equations (or other equations for that matter). From this simulation state, we ask for the shared variable (our node voltages), and use those variables to get some elements inside the Y-matrix and RHS-vector.

We need 6 elements in total:

- $Y_{A,A} = \frac{\partial i_A}{\partial v_A}$
- $Y_{A,B} = \frac{\partial i_A}{\partial v_B}$
- $Y_{B,A} = \frac{\partial i_B}{\partial v_A}$
- $Y_{B,B} = \frac{\partial i_B}{\partial v_B}$
- $RHS_A = i_A - \frac{\partial i_A}{\partial v_A}\cdot v_A - \frac{\partial i_A}{\partial v_B}\cdot v_B$
- $RHS_B = i_B - \frac{\partial i_B}{\partial v_A}\cdot v_A - \frac{\partial i_B}{\partial v_B}\cdot v_B$

Since $i_B = -i_A = i_D$, and since $\frac{\partial i_A}{\partial v_A} = \frac{\partial i_B}{\partial v_B} = -\frac{\partial i_A}{\partial v_B} = -\frac{\partial i_B}{\partial v_A} = \frac{i_D}{\eta\cdot V_T} = g$, the result is actually a bit simpler to write:

- $Y_{A,A} = g$
- $Y_{A,B} = -g$
- $Y_{B,A} = -g$
- $Y_{B,B} = g$
- $RHS_A = -(i_D - g\cdot (v_A-v_B))$
- $RHS_B = i_D - g\cdot (v_A-v_B)$

These 6 contributions are potentially computed many times, given that the simulation will converge *iteratively* to a solution. So it is best that you try to keep the calculation time of these contributions to the Y-matrix and RHS-vector as fast as possible. Of course, this also depends on how accurate you want the model to be. The more modern transistor models for example, quickly need multiple pages of code to just model its behavior accurately.

Also note that we are using the property `Vte`, which was calculated by the temperature behavior to avoid some computation time in this heavily used loading method.