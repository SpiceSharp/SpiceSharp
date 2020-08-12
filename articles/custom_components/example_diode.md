# Example of MNA - Diodes

A diode is an example of a *nonlinear* component. When connected between two nodes A and B, we can use the [Shockley diode model](https://en.wikipedia.org/wiki/Diode_modelling) to model the voltage-current relationship.

<p align="center"><img src="images/example_circuit_mna_dio.svg" alt="Diode definition" width="100px" /></p>

$$i_D = I_{SS}\left(e^\frac{v_A-v_B}{\eta V_T}\right)$$

The diode current flows *out* of node A, and *into* node B, so we have contributions to *two* current equations.

$$\begin{aligned}
f_A(...,v_A,...,v_B,...) &= +i_D = I_S\left(e^{\frac{v_A-v_B}{nV_T}}-1\right)\\\\
f_B(...,v_A,...,v_B,...) &= -i_D=-I_S\left(e^{\frac{v_A-v_B}{nV-T}}-1)\right)
\end{aligned}$$

From this we can calculate the contributions to the *Y-matrix*. Since the current equation contributions only depend on nodes A and B, we only get contributions to four elements.

$$\begin{aligned}
Y_{A,A} &= \left.\frac{\partial f_A}{\partial v_A}\right|^{(k)}=\frac{I_S}{nV_T}e^{\frac{v_A^{(k)}-v_B^{(k)}}{nV_T}} &= &+g_D\\\\
Y_{A,B} &= \left.\frac{\partial f_A}{\partial v_B}\right|^{(k)}=-\frac{I_S}{nV_T}e^{\frac{v_A^{(k)}-v_B^{(k)}}{nV_T}} &= &-g_D\\\\
Y_{B,A} &= \left.\frac{\partial f_B}{\partial v_A}\right|^{(k)}=-\frac{I_S}{nV_T}e^{\frac{v_A^{(k)}-v_B^{(k)}}{nV_T}} &= &-g_D\\\\
Y_{B,B} &= \left.\frac{\partial f_B}{\partial v_B}\right|^{(k)}=\frac{I_S}{nV_T}e^{\frac{v_A^{(k)}-v_B^{(k)}}{nV_T}} &= &+g_D
\end{aligned}$$

We then calculate the contributions to the *RHS-vector*:

$$\begin{aligned}
RHS_A &= \pmb J_A\pmb x^{(k)} - f_A(...,v_A^{(k)},...,v_B^{(k)},...)\\\\
&= g_D\cdot (v_A^{(k)}-v_B^{(k)}) - I_S\left(e^\frac{v_A^{(k)}-v_B^{(k)}}{nV_T}-1\right)\\\\
&= +c_D\\\\
RHS_B &= f_B(...,v_A^{(k)},...,v_B^{(k)},...)-\pmb J_B\pmb x^{(k)}\\\\
&= -\left(g_D\cdot (v_A^{(k)}-v_B^{(k)}) - I_S\left(e^\frac{v_A^{(k)}-v_B^{(k)}}{nV_T}-1\right)\right)\\\\
&= -c_D
\end{aligned}$$

We note that this time the *RHS-vector* contributions are *not* 0 for the current equations. This is again typical for *nonlinear* components. The solution will need to be found in multiple iterations.
