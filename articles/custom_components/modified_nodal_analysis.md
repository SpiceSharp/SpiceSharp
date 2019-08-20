# Modified Nodal Analysis

Before starting to build custom components, it is first important to discuss how the simulator solves the node voltages of a circuit. This is done using a modified nodal analysis.

## Nodal Analysis

Regular *nodal analysis* works by describing Kirchoff's Current Laws (KCL) in each and every node of the circuit. Let's take a look at the following circuit.

<p align="center"><img src="images/example_circuit_mna.svg" alt="Example circuit" /></p>

This circuit contains two types of elements:

- *Current sources* do not care about the voltage across them and will always drive the specified current.
- *Resistors* obey Ohm's law, such that:

<p align="center"><a href="https://www.codecogs.com/eqnedit.php?latex=v_R&space;=&space;R\cdot&space;i_R"><img src="https://latex.codecogs.com/svg.latex?v_R&space;=&space;R\cdot&space;i_R" /></a></a></p>

*Kirchoff's Current Law* states that the *sum* of all *currents* in every *node* has to equal *zero*. In this circuit, we have 3 nodes, and so we get a system of 3 equations:

<p align="center"><img src="https://latex.codecogs.com/svg.latex?\left\{\begin{matrix}&space;&&space;-1A&space;&plus;&space;\frac{v_1&space;-&space;v_2}{5\Omega}&space;=&space;0&space;\\&space;&&space;\frac{v_2&space;-&space;v_1}{5\Omega}&space;&plus;&space;\frac{v_2}{10\Omega}&space;&plus;&space;\frac{v_2&space;-&space;v_3}{7\Omega}&space;=&space;0&space;\\&space;&&space;\frac{v_3&space;-&space;v_2}{7\Omega}&space;-&space;1.5A&space;=&space;0&space;\end{matrix}\right." /></a></p>

This can conveniently be written in matrix formulation as well:

<p align="center"><img src="https://latex.codecogs.com/svg.latex?\begin{pmatrix}&space;\frac{1}{5\Omega}&space;&&space;-\frac{1}{5\Omega}&space;&&space;0&space;\\&space;-\frac{1}{5\Omega}&space;&&space;\frac{1}{5\Omega}&plus;\frac{1}{7\Omega}&plus;\frac{1}{10\Omega}&space;&&space;-\frac{1}{7\Omega}&space;\\&space;0&space;&&space;-\frac{1}{7\Omega}&space;&&space;\frac{1}{7\Omega}&space;\end{pmatrix}&space;\begin{pmatrix}&space;v_1&space;\\&space;v_2&space;\\&space;v_3&space;\end{pmatrix}&space;=&space;\begin{pmatrix}&space;1A&space;\\&space;0&space;\\&space;1.5A&space;\end{pmatrix}" /></a></p>

The 3x3 matrix on the left is called the *admittance matrix*, while the vector on the right is the current vector. Pretty much *all* circuit simulation software will formulate a problem in terms of a *matrix* and a *vector*, for which the simulator will then solve <img src="https://latex.codecogs.com/svg.latex?%5Cpmb%20G%5Ccdot%20%5Cpmb%20v%20%3D%20%5Cpmb%20i" alt="G*v=i" />.

## Modified Nodal Analysis

What happens if we introduce a *voltage source* into the circuit?

<p align="center"><img src="images/example_circuit_mna_2.svg" alt="Example circuit" /></p>

We can't use the KCL equation now, because it is in the nature of a voltage source to *not care* about currents! This is where we get creative. We define a new unknown variable, the current through the voltage source ![i_V](https://latex.codecogs.com/svg.latex?\inline&space;i_V). We also add the equation of the voltage source to the system of equations:

<p align="center"><a href="https://www.codecogs.com/eqnedit.php?latex=v_1&space;=&space;1V"><img src="https://latex.codecogs.com/svg.latex?v_1&space;=&space;1V" /></a></p>

And what we find is the useful result: we can combine it all using a matrix and vector again!

<p align="center"><img src="https://latex.codecogs.com/svg.latex?\begin{pmatrix}&space;\frac{1}{5\Omega}&space;&&space;-\frac{1}{5\Omega}&space;&&space;0&space;&&space;1&space;\\&space;-\frac{1}{5\Omega}&space;&&space;\frac{1}{5\Omega}&space;&plus;&space;\frac{1}{10\Omega}&space;&plus;&space;\frac{1}{7\Omega}&space;&&space;-\frac{1}{7\Omega}&space;&&space;0&space;\\&space;0&space;&&space;-\frac{1}{7\Omega}&space;&&space;\frac{1}{7\Omega}&space;&&space;0&space;\\&space;1&space;&&space;0&space;&&space;0&space;&&space;0&space;\end{pmatrix}&space;\begin{pmatrix}&space;v_1&space;\\&space;v_2&space;\\&space;v_3&space;\\&space;i_V&space;\end{pmatrix}&space;=&space;\begin{pmatrix}&space;0&space;\\&space;0&space;\\&space;1.5A&space;\\&space;1V&space;\end{pmatrix}" /></p>

We notice the following properties:
- We added an unknown *current*. For *regular* Nodal Analysis, the unknowns are *always a voltage*.
- The circuit got *larger* because we added a voltage source. While this may not be desirable when working it out by hand, a simulator will rarely feel the difference.
- Each component of the circuit has its own unique contribution to the admittance matrix and current vector. This is a direct consequence of KCL that says that the **sum** of currents needs to total to 0. In fact, the resistors only affect the elements to which they are connected to - ie. the nodes where they add or subtract current. For example, the 5 ohm resistor only affects the rows and columns of ![v_1](https://www.codecogs.com/svg.latex?\inline&space;v_1) and ![v_2](https://www.codecogs.com/svg.latex?\inline&space;v_2). This turns out to be an important general property! A Spice simulator will give the matrix and vector to each component, and the component will *stamp* the matrix and vector with contributions that depend *only* on that component!

## Nonlinear components

The electronics world is littered with so-called nonlinear components. These are components where the currents and voltages do not relate *linearly*, but are often connected in complex ways.

A resistor is a *linear* component, because the current and voltage are connected via Ohm's law: ![v = R\*i](https://latex.codecogs.com/svg.latex?\inline&space;v=R\cdot i). However, a diode is a *non-linear* component, because the diode current depends on the diode voltage following the equation ![i = I_s(e^{qV/nkT}-1)](https://latex.codecogs.com/svg.latex?\inline&space;i=I_{ss}\left(e^{\frac{qV}{nkT}}-1\right)).

In order to solve a circuit with nonlinear components, we have to resort to *iterative* algorithms. Spice-based simulators almost exclusively use the **Newton-Raphson** algorithm. This algorithm tries to solve, generally speaking, the following problem:

<p align="center"><img src="https://latex.codecogs.com/svg.latex?\left\{&space;\begin{matrix}&space;f_1(x_1,&space;x_2,&space;...,&space;x_n)&space;&&space;=&space;0&space;\\&space;\vdots&space;&&space;=&space;0&space;\\&space;f_n(x_1,&space;x_2,&space;...,&space;x_n)&space;&&space;=&space;0&space;\end{matrix}\right.&space;\&space;\text{or}\&space;\pmb&space;F(\pmb&space;X)&space;=&space;0" /></p>

For this problem, the functions ![f_1, f_2, ..., f_n](https://latex.codecogs.com/svg.latex?\inline&space;f_1%2C%20f_2%2C%20...%2C%20f_n) do *not* have to be linear! The algorithm then shows that, starting from an initial vector ![x^(0)](https://latex.codecogs.com/svg.latex?\inline&space;%5Cpmb%20x%5E%7B%280%29%7D), a *new* vector, ![x^(1)](https://latex.codecogs.com/svg.latex?\inline&space;%5Cpmb%20x%5E%7B%281%29%7D) can be approximated that is *closer* to the real solution. The new solution ![x^(1)](https://latex.codecogs.com/svg.latex?\inline&space;%5Cpmb%20x%5E%7B%281%29%7D) can be found by solving the following set of equations.

<p align="center"><img src="https://latex.codecogs.com/svg.latex?\pmb&space;J(\pmb&space;x^{(0)})\cdot\Delta\pmb&space;x^{(1)}=\pmb&space;F(\pmb&space;x^{(0)})&space;\Rightarrow&space;\pmb&space;x^{(1)}&space;=&space;\pmb&space;x^{(0)}&plus;\Delta\pmb&space;x^{(1)}" /></p>

Where ![J(x^(0))](https://latex.codecogs.com/svg.latex?\inline&space;%5Cpmb%20J%28%5Cpmb%20x%5E%7B%280%29%7D%29) is called the *Jacobian*, which is

<p align="center"><img src="https://latex.codecogs.com/svg.latex?\pmb&space;J(\pmb&space;x)&space;=&space;\begin{pmatrix}&space;\frac{\partial&space;f_1}{\partial&space;x_1}&space;&&space;\hdots&space;&&space;\frac{\partial&space;f_1}{x_n}&space;\\&space;\hdots&space;&&space;\ddots&space;&&space;\hdots&space;\\&space;\frac{\partial&space;f_n}{\partial&space;x_1}&space;&&space;\hdots&space;&&space;\frac{\partial&space;f_n}{x_n}&space;\end{pmatrix}" /></p>

We can repeat the process using ![x^(1)](https://latex.codecogs.com/svg.latex?\inline&space;\pmb&space;x^{(1)}) as a starting solution, to get even *closer* to the real position. As we repeat this algorithm, the solution will *converge* to the real solution. Once our solution is almost indistinguishably close to the real solution, we accept it as our final solution.

One more thing to note is that Spice will modify the algorithm a tiny bit.

<p align="center"><img src="https://latex.codecogs.com/svg.latex?\begin{align*}&space;\pmb&space;J(\pmb&space;x^{(0)})\cdot\Delta\pmb&space;x^{(k+1)}&space;&=&space;\pmb&space;F(\pmb&space;x^{(k)})&space;\\&space;&\Downarrow&space;\\&space;\pmb&space;J(\pmb&space;x^{(k)})\cdot\left(\pmb&space;x^{(k+1)}-\pmb&space;x^{(k)}\right)&space;&=&space;\pmb&space;F(\pmb&space;x^{(k)})&space;\\&space;&&space;\Downarrow&space;\\&space;\pmb&space;J(\pmb&space;x^{(k)})\cdot&space;\pmb&space;x^{(k+1)}&space;&=&space;\pmb&space;F(\pmb&space;x^{(k)})&space;-&space;\pmb&space;J(\pmb&space;x^{(k)})\cdot\pmb&space;x^{(k)}&space;\end{align*}" /></p>

The *Jacobian* is from here on out called the *Y-matrix*. Everything on the right of the equation is called the *Right-Hand Side vector* (RHS vector). This formulation allows us to immediately calculate the *next* solution rather than the increment to find the next solution.

## How Spice# does it

Spice# will give each behavior the chance to:
- allocate elements in the Y-matrix and RHS-vector during binding (in the method *[Bind](xref:SpiceSharp.Behaviors.IBehavior.Bind(SpiceSharp.Simulations.Simulation,SpiceSharp.Behaviors.BindingContext))*). To optimize performance and memory, the behavior should try not allocate more elements than needed.
- add contributions to the allocated elements when computing a new iteration. This is called *loading* the Y-matrix and RHS-vector. After all components have loaded the matrix and vector, the simulator will solve the system of equations to find the solution for this iteration.

### Example: Resistors

As discussed in the first section, a resistor follows Ohm's law. Let us assume that the resistor is connected between two nodes A and B.

<p align="center"><img src="images/example_circuit_mna_res.svg" width="100px" alt="Resistor definition" /></p>

Then Ohm's law dictates that

<p align="center"><img src="https://latex.codecogs.com/svg.latex?\frac{v_A-v_B}{R}=i_R" /></p>

The resistor current ![i_R](https://latex.codecogs.com/svg.latex?\inline&space;i_R) will flow *out* of node A, and *into* node B, so we have contributions to row A and row B:

<p align="center"><img src="https://latex.codecogs.com/svg.latex?\begin{align*}&space;f_A(...,v_A,...,v_B,...)=&plus;i_R&space;&=&&space;\frac{1}{R}v_A&space;&-\frac{1}{R}v_B\\&space;f_B(...,v_A,...,v_B,...)=-i_R&space;&=&&space;-\frac{1}{R}v_A&space;&&plus;\frac{1}{R}v_B&space;\end{align*}" /></p>

Now we can compute the contributions to the *Y-matrix*:

<img src="https://latex.codecogs.com/svg.latex?\begin{align*}&space;Y_{A,A}&=\frac{\partial&space;f_A}{\partial&space;v_A}=\frac{1}{R}\\&space;Y_{A,B}&=\frac{\partial&space;f_A}{\partial&space;v_B}=-\frac{1}{R}\\&space;Y_{B,A}&=\frac{\partial&space;f_B}{\partial&space;v_A}=-\frac{1}{R}\\&space;Y_{B,B}&=\frac{\partial&space;f_B}{\partial&space;v_B}=\frac{1}{R}&space;\end{align*}" />

All other *Y-matrix* contributions are 0. Similarly, we calculate the contributions to the *RHS vector*:

<img src="https://latex.codecogs.com/svg.latex?\begin{align*}&space;RHS_A&=f_A(...,v_A^{(k)},...,v_B^{(k)},...)-\pmb&space;J_A\pmb&space;x^{(k)}\\&space;&=\frac{v_A}{R}-\frac{v_B}{R}-\left(\frac{v_A}{R}-\frac{v_B}{R}\right&space;)\\&space;&=0\\&space;RHS_B&=f_B(...,v_A^{(k)},...,v_B^{(k)},...)-\pmb&space;J_B\pmb&space;x^{(k)}\\&space;&=\frac{v_B}{R}-\frac{v_A}{R}-\left(\frac{v_B}{R}-\frac{v_A}{R}\right&space;)\\&space;&=0&space;\end{align*}" />

The first and second terms for the RHS vector cancel each other out. This turns out to be a *general property of linear components*. Another consequence is that a step of the iterative method is identical to regular Nodal Analysis, and we only need *one* iteration to find the right solution *if we only have linear components*. Once a nonlinear component is introduced, a single iteration will not be sufficient anymore.

To summarize:
- During binding, the *Resistor* biasing behavior will allocate 4 matrix elements: ![matrix elements](https://latex.codecogs.com/svg.latex?\inline&space;Y_{A,A},Y_{A,B},Y_{B,A},Y_{B,B}). It does not need RHS vector elements because they are always 0.
- During execution, the *Resistor* biasing behavior will add values to the following elements:

  ![Contributions](https://latex.codecogs.com/svg.latex?%5Cbegin%7Balign*%7D%20Y_%7BA%2CA%7D%26%5Cmathrel%7B&plus;%7D%3D%5Cfrac%7B1%7D%7BR%7D%5C%5C%20Y_%7BA%2CB%7D%26%5Cmathrel%7B-%7D%3D%5Cfrac%7B1%7D%7BR%7D%5C%5C%20Y_%7BB%2CA%7D%26%5Cmathrel%7B-%7D%3D%5Cfrac%7B1%7D%7BR%7D%5C%5C%20Y_%7BB%2CB%7D%26%5Cmathrel%7B&plus;%7D%3D%5Cfrac%7B1%7D%7BR%7D%20%5Cend%7Balign*%7D)

- The simulator will give all other components in the circuit a chance to load the matrix with its contributions.

### Example: Diodes

A diode is an example of a *nonlinear* component. When connected between two nodes A and B, we can use the [Shockley diode model](https://en.wikipedia.org/wiki/Diode_modelling) to model the voltage-current relationship.

<p align="center"><img src="images/example_circuit_mna_dio.svg" alt="Diode definition" width="100px" /></p>

<p align="center"><img src="https://latex.codecogs.com/svg.latex?i_D=I_S\left(e^{\frac{v_A-v_B}{nV_T}}-1\right)" /></p>

The diode current flows *out* of node A, and *into* node B, so we have contributions to *two* current equations.

<img src="https://latex.codecogs.com/svg.latex?\begin{align*}&space;f_A(...,v_A,...,v_B,...)&=&plus;i_D=I_S\left(e^{\frac{v_A-v_B}{nV_T}}-1\right)\\&space;f_B(...,v_A,...,v_B,...)&=-i_D=-I_S\left(e^{\frac{v_A-v_B}{nV-T}}-1)\right)&space;\end{align*}" />

From this we can calculate the contributions to the *Y-matrix*. Since the current equation contributions only depend on nodes A and B, we only get contributions to four elements.

<img src="https://latex.codecogs.com/svg.latex?\begin{align*}&space;Y_{A,A}&=\left.\frac{\partial&space;f_A}{\partial&space;v_A}\right|^{(k)}=\frac{I_S}{nV_T}e^{\frac{v_A^{(k)}-v_B^{(k)}}{nV_T}}&=&&plus;g_D\\&space;Y_{A,B}&=\left.\frac{\partial&space;f_A}{\partial&space;v_B}\right|^{(k)}=-\frac{I_S}{nV_T}e^{\frac{v_A^{(k)}-v_B^{(k)}}{nV_T}}&=&-g_D\\&space;Y_{B,A}&=\left.\frac{\partial&space;f_B}{\partial&space;v_A}\right|^{(k)}=-\frac{I_S}{nV_T}e^{\frac{v_A^{(k)}-v_B^{(k)}}{nV_T}}&space;&=&-g_D\\&space;Y_{B,B}&=\left.\frac{\partial&space;f_B}{\partial&space;v_B}\right|^{(k)}=\frac{I_S}{nV_T}e^{\frac{v_A^{(k)}-v_B^{(k)}}{nV_T}}&=&&plus;g_D&space;\end{align*}" />

We then calculate the contributions to the *RHS-vector*:

<img src="https://latex.codecogs.com/svg.latex?\begin{align*}&space;RHS_A&=&space;f_A(...,v_A^{(k)},...,v_B^{(k)},...)-\pmb&space;J_A\pmb&space;x^{(k)}\\&space;&=&space;I_S\left(e^\frac{v_A-v_B}{nV_T}-1\right)-g_D(v_A-v_B)\\&space;&=&space;&plus;c_D\\&space;RHS_B&=&space;f_B(...,v_A^{(k)},...,v_B^{(k)},...)-\pmb&space;J_B\pmb&space;x^{(k)}\\&space;&=&space;-\left(I_S\left(e^\frac{v_A-v_B}{nV_T}-1\right)-g_D(v_A-v_B)\right)\\&space;&=&space;-c_D&space;\end{align*}" />

We note that this time the *RHS-vector* contributions are *not* 0 for the current equations. This is again typical for *nonlinear* components. The solution will need to be found in multiple iterations.

<div class="pull-left">[Previous: Custom Models](custom_models.md)</div> <div class="pull-right">[Next: Base Behaviors](base_behaviors.md)</div>
