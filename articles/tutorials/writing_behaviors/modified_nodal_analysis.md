# Modified Nodal Analysis

Before starting to build custom components, it is first important to discuss how the simulator solves the node voltages of a circuit. This is done using a modified nodal analysis.

## Nodal Analysis

Regular *nodal analysis* works by describing Kirchoff's Current Laws (KCL) in each and every node of the circuit. Let's take a look at the following circuit.

<p align="center"><img src="images/example_circuit_mna.svg" alt="Example circuit" /></p>

This circuit contains two types of elements:

- *Current sources* do not care about the voltage across them and will always drive the specified current.
- *Resistors* obey Ohm's law, such that \\(v_R = R\cdot i_R\\).

*Kirchoff's Current Law* states that the *sum* of all *currents* in every *node* has to equal *zero*. In this circuit, we have 3 nodes, and so we get a system of 3 equations:

$$\left\{\begin{matrix} & -1A + \frac{v_1 - v_2}{5\Omega} = 0 \\ & \frac{v_2 - v_1}{5\Omega} + \frac{v_2}{10\Omega} + \frac{v_2 - v_3}{7\Omega} = 0 \\ & \frac{v_3 - v_2}{7\Omega} - 1.5A = 0 \end{matrix}\right.$$

This can conveniently be written in matrix formulation as well:

$$\begin{pmatrix}
  \frac{1}{5\Omega} & -\frac{1}{5\Omega} & 0 \\\\
  -\frac{1}{5\Omega} & \frac{1}{5\Omega}+\frac{1}{7\Omega}+\frac{1}{10\Omega} & -\frac{1}{7\Omega} \\\\
  0 & -\frac{1}{7\Omega} & \frac{1}{7\Omega}
\end{pmatrix}
\begin{pmatrix}
  v_1 \\\\
  v_2 \\\\
  v_3
\end{pmatrix} = 
\begin{pmatrix}
  1A \\\\
  0 \\\\
  1.5A
\end{pmatrix}$$

The 3x3 matrix on the left is called the *admittance matrix*, while the vector on the right is the current vector. Pretty much *all* circuit simulation software will formulate a problem in terms of a *matrix* and a *vector*, for which the simulator will then solve \\(G\cdot v = i\\).

## Modified Nodal Analysis

What happens if we introduce a *voltage source* into the circuit?

<p align="center"><img src="images/example_circuit_mna_2.svg" alt="Example circuit" /></p>

We can't use the KCL equation now, because it is in the nature of a voltage source to *not care* about currents! This is where we get creative. We define a new unknown variable, the current through the voltage source \\(i_V\\). We also add the equation of the voltage source to the system of equations:

$$v_1 = 1V$$

And what we find is the useful result: we can combine it all using a matrix and vector again!

$$\begin{pmatrix}
  \frac{1}{5\Omega} & -\frac{1}{5\Omega} & 0 & 1\\\\
  -\frac{1}{5\Omega} & \frac{1}{5\Omega} + \frac{1}{10\Omega} + \frac{1}{7\Omega} & -\frac{1}{7\Omega} & 0\\\\
  0 & -\frac{1}{7\Omega} & \frac{1}{7\Omega} & 0\\\\
  1 & 0 & 0 & 0
\end{pmatrix}
\begin{pmatrix} 
  v_1 \\\\
  v_2 \\\\
  v_3 \\\\
  i_V
\end{pmatrix} = 
\begin{pmatrix}
  0 \\\\
  0 \\\\
  1.5A \\\\
  1V
\end{pmatrix}$$

We notice the following:
- We added an unknown *current*. For *regular* Nodal Analysis, the unknowns were always a *voltage*.
- The circuit got *larger* because we added a voltage source. While this may not be desirable when working out the equations by hand, a simulator will not really feel the difference.
- Each component in the circuit has its own unique contribution to the admittance matrix and current vector. This is a direct consequence of KCL that says that the **sum** of currents needs to total to 0. In fact, the resistors only affect the elements to which they are connected to - ie. the nodes where they push in or take away current. For example, the 5 ohm resistor only affects the rows and columns of \\(v_1\\) and \\(v_2\\). This turns out to be an important general property! A Spice simulator will give the matrix and vector to each component, and the component will *stamp* the matrix and vector with contributions that depend *only* on that component!

## Nonlinear components

The electronics world is littered with so-called nonlinear components. These are components where the currents and voltages do not relate *linearly*, but are often connected in complex ways.

A resistor is a *linear* component, because the current and voltage are connected via Ohm's law: $v = R\cdot i$. However, a diode is a *non-linear* component, because the diode current depends on the diode voltage following the equation $i = I_{ss}(e^{qV/\eta kT} - 1)$.

In order to solve a circuit with nonlinear components, we have to resort to *iterative* algorithms. Spice-based simulators almost exclusively use the **Newton-Raphson** algorithm. This algorithm tries to solve, generally speaking, the following problem:

$$\left\\{\begin{matrix}
f_1(x_1, x_2, ..., x_n) &= 0 \\\\
\vdots &= 0 \\\\
f_n(x_1, x_2, ..., x_n) &= 0 \\\\
\end{matrix}\right. $$

The notations in bold have multiple elements.

For this problem, the functions $f_1, f_2, ..., f_n$ do *not* have to be linear! The algorithm then shows that, starting from an initial vector $\pmb x^{(0)}$, a *new* vector, $\pmb x^{(1)}$ can be approximated that is *closer* to the real solution. The new solution $x^{(1)}$ can be found by solving the following set of equations.

$$\pmb J(\pmb x^{(0)})\cdot\Delta\pmb x^{(1)} = -\pmb F(\pmb x^{(0)}) \Rightarrow \pmb x^{(1)} = \pmb x^{(0)}+\Delta\pmb x^{(1)}$$

Where $\pmb J(x^{(0)})$ is called the *Jacobian*, which is

$$\pmb J(\pmb x) = 
\begin{pmatrix} 
  \frac{\partial f_1}{\partial x_1} & \frac{\partial f_1}{\partial x_2} & \dots & \frac{\partial f_1}{x_n} \\\\
  \frac{\partial f_2}{\partial x_1} & \frac{\partial f_2}{\partial x_2} & \dots & \frac{\partial f_2}{x_n} \\\\
  \vdots & \vdots & \ddots & \vdots \\\\
  \frac{\partial f_n}{\partial x_1} & \frac{\partial f_n}{\partial x_2} & \dots & \frac{\partial f_n}{x_n}
\end{pmatrix}$$

We can repeat the process using \\(\pmb x^{(1)}\\) as a starting solution, to get even *closer* to the real position. As we repeat this algorithm, the solution will *converge* to the real solution. Once our solution is almost indistinguishably close to the real solution, we accept it as our final solution.

One more thing to note is that Spice will modify the algorithm a tiny bit.

$$\begin{aligned}
\pmb J(\pmb x^{(0)})\cdot\Delta\pmb x^{(k+1)} & = -\pmb F(\pmb x^{(k)}) \\\\
& \Downarrow \\\\
\pmb J(\pmb x^{(k)})\cdot\left(\pmb x^{(k+1)}-\pmb x^{(k)}\right) & = -\pmb F(\pmb x^{(k)}) \\\\
& \Downarrow \\\\
\pmb J(\pmb x^{(k)})\cdot \pmb x^{(k+1)} & = -\pmb F(\pmb x^{(k)}) + \pmb J(\pmb x^{(k)})\cdot\pmb x^{(k)} \\
\end{aligned}$$

The *Jacobian* is from here on out called the *Y-matrix*. Everything on the right of the equation is called the *Right-Hand Side vector* (RHS vector). This formulation allows us to immediately calculate the *next* solution rather than the increment to find the next solution.

## How Spice# does it

Spice# will give each behavior the chance to add contributions to the allocated elements when computing a new iteration. This is called *loading* the Y-matrix and RHS-vector. After all components have loaded the matrix and vector, the simulator will solve the system of equations to find the solution for this iteration.
