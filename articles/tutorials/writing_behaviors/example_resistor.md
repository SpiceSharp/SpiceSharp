# Example of MNA - Resistors

A resistor follows Ohm's law. Let us assume that the resistor is connected between two nodes A and B.

<p align="center"><img src="images/example_circuit_mna_res.svg" width="100px" alt="Resistor definition" /></p>

Then Ohm's law dictates that

$$\frac{v_A - v_B}{R} = i_R$$

The resistor current \\(i_R\\) will flow *out* of node A, and *into* node B, so we have contributions to row A and row B:

$$\begin{aligned}
f_A(...,v_A,...,v_B,...) = +i_R &=& \frac{1}{R}v_A &-\frac{1}{R}v_B\\\\
f_B(...,v_A,...,v_B,...) = -i_R &=& -\frac{1}{R}v_A &+\frac{1}{R}v_B\\
\end{aligned}$$

Our convention is that a current flowing *out* of a node is *positive*. Now we can compute the contributions to the *Y-matrix*:

$$\begin{aligned}
Y_{A,A} &= \frac{\partial f_A}{\partial v_A} = \frac{1}{R}\\\\
Y_{A,B} &= \frac{\partial f_A}{\partial v_B} = -\frac{1}{R}\\\\
Y_{B,A} &= \frac{\partial f_B}{\partial v_A} = -\frac{1}{R}\\\\
Y_{B,B} &= \frac{\partial f_B}{\partial v_B} = \frac{1}{R}
\end{aligned}$$

All other *Y-matrix* contributions are 0. Similarly, we calculate the contributions to the *RHS vector*:

$$\begin{aligned}
RHS_A &= \pmb J_A\pmb x^{(k)} - f_A(...,v_A^{(k)},...,v_B^{(k)},...)\\\\
&=\frac{v_A}{R}-\frac{v_B}{R}-\left(\frac{v_A}{R}-\frac{v_B}{R}\right)\\\\
&=0\\\\
RHS_B &= \pmb J_B\pmb x^{(k)} - f_B(...,v_A^{(k)},...,v_B^{(k)},...)\\\\
&=\frac{v_B}{R}-\frac{v_A}{R}-\left(\frac{v_B}{R}-\frac{v_A}{R}\right)\\\\
&=0
\end{aligned}$$

The first and second terms for the RHS vector cancel each other out. This turns out to be a *general property of linear components*. Another consequence is that a step of the iterative method is identical to regular Nodal Analysis, and we only need *one* iteration to find the right solution *if we only have linear components*. Once a nonlinear component is introduced, a single iteration will not be sufficient anymore.

To summarize:
- During the creation of the **[IBiasingBehavior](xref:SpiceSharp.Behaviors.IBiasingBehavior)** for a resistor, 4 Y-matrix elements are allocated: \\(Y_{A,A}, Y_{A,B}, Y_{B,A}, Y_{B,B}\\). It does not need RHS vector elements because they are always 0.
- When loading the Y-matrix, the behavior will add the following values to these matrix elements.

$$\begin{aligned}
Y_{A,A} &+= \frac{1}{R} \\\\
Y_{A,B} &-= \frac{1}{R} \\\\
Y_{B,A} &-= \frac{1}{R} \\\\
Y_{B,B} &+= \frac{1}{R}
\end{aligned}$$

- The simulator will give all other components in the circuit a chance to load the matrix with their contributions.