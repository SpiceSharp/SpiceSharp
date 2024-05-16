# Parallelization

Spice# supports a few options for solving circuits in a parallel way, but they are always centered around the **[Parallel](xref:SpiceSharp.Components.Parallel)** entity. The parameters of this component contain a dictionary, *[WorkDistributors](xref:SpiceSharp.Components.ParallelComponents.Parameters.WorkDistributors)*, whose contents will decide which behaviors are run in parallel and which ones won't. In order to run behaviors in parallel, you should register a work distributor for the given behavior interface type.

There is some overhead associated with parallelization. The entity will avoid two behaviors accessing the same matrix and vector elements of a solver at the same time. There is inevitably some overhead associated with this, as well as distributing the work to different threads.

Using the entity works best in conjunction with entities that are heavily CPU limited. For most "simple" components (e.g. resistors, capacitors, etc.), most of the time would be spent dealing with simultaneously accessing shared memory. However, if there are many *complex* models (e.g. some bipolar or mosfet model computations) then it is worth a shot.

If it is possible to group the circuit in rather large subcircuits, it may be useful to try **local solvers** instead, even if those subcircuits contain many "simple" components.

## Local solvers

Subcircuits can be configured to use their own local solver with a Y-matrix and right-hand side vector. This causes the subcircuit to first compute a Norton equivalent circuit with respect to all of the ports of the subcircuit (provided that they can be computed). It then loads the global solver with the computed Norton equivalent contributions.

In order to maximize the performance gain, you must make sure that there are many subcircuits that can be parallelized. Also, each of these subcircuits should contain many internal nodes, and preferably only a few ports in comparison. In worst case, if you use subcircuits with no internal nodes, then the global matrix will still be the same size, and all overhead needed for making local solvers work will only reduce the overall performance.

### How does it work?

If you are interested in how the math works out, this is your turn. Let us look at the *global* Y-matrix and right-hand side vector as follows:

$$
\left(
\begin{matrix}
A_{11} & A_{12} & 0 \\
A_{21} & A_{22} + B_{11} & B_{12} \\
0 & B_{21} & B_{22}
\end{matrix}\right) \left(\begin{matrix}
X_A \\
X_S \\
X_B
\end{matrix}\right) = \left(\begin{matrix}
Y_{A1} \\
Y_{A2} + Y_{B1} \\
Y_{B2}
\end{matrix}\right)
$$

The submatrices labeled $A_{ij}$ and $Y_{Ai}$ all contain contributions from the subcircuit. All submatrices labeled as $B_{ij}$ and $Y_{Bi}$ contain contributions from the global circuit. What we want to do is to first (at least partially) solve $A$ locally, and then solve $B$ afterwards. $X_A$ is the solution for all *internal* nodes to the subcircuit, $X_S$ refers to all *shared* nodes between the subcircuit and the global circuit (the ports), and $X_B$ are only part of the global circuit.

Lets say we have a local solver, which only contains the contributions from $A$. After a few LU decomposing steps, we have the following equivalence:

$$
\left(\begin{matrix}
A_{11} & A_{12} \\
A_{21} & A_{22}
\end{matrix}\right) = \left(\begin{matrix}
L_A & 0 \\
A_{21}' & I
\end{matrix}\right) \left(\begin{matrix}
U_A & A_{12}' \\
0 & A_{22}'
\end{matrix}\right)
$$

We have the following system of equations, with $X_A$ and $X_S'$ unknown.

$$
\left(\begin{matrix}
L_A & 0 \\
A_{21}' & I
\end{matrix}\right) \left(\begin{matrix}
U_A & A_{12}' \\
0 & A_{22}'
\end{matrix}\right) \left(\begin{matrix}
X_A \\
X_S'
\end{matrix}\right) = \left(\begin{matrix}
Y_{A1} \\
Y_{A2}
\end{matrix}\right)
$$

Writing out these as equations, we find

$$
\left\{\begin{align*}
L_A (U_A X_A + A_{12}' X_S) &= Y_{A1} \\
A_{21}' U_A + A_{22}' &= Y_{A2}
\end{align*}\right.
$$

If we take a look at the first equation, we see that we can actually solve the equation for $X_A' = U_A X_A + A_{12}' X_S$ by doing a forward substitution. Please remember this factor as it reappears later.

Let's now take a look what would happen if were to only partially decompose the global matrix. This would result in:

$$
\left(
\begin{matrix}
L_{A} & 0 & 0 \\
A_{21}' & I & 0 \\
0 & 0 & I
\end{matrix}\right) \left(
\begin{matrix}
U_A & A_{12}' & 0 \\
0 & A_{22}' + B_{11}' & B_{12} \\
0 & B_{21} & B_{22}
\end{matrix}\right) \left(\begin{matrix}
X_A \\
X_S \\
X_B
\end{matrix}\right) = \left(\begin{matrix}
Y_{A1} \\
Y_{A2} + Y_{B1} \\
Y_{B2}
\end{matrix}\right)
$$

Writing out these as equations, we have

$$
\left\{\begin{align*}
L_A (U_A X_A + A_{12}' X_S) &= Y_{A1} \\
A_{21}' U_A X_A + (A_{21}' A_{12}' + A_{22}' + B_{11}') X_S + B_{12} X_B &= Y_{A2} + Y_{B1} \\
B_{21} X_S + B_{22} X_B &= Y_{B2}
\end{align*}\right.
$$

The first equation is actually identical to that of the local solver. The second equation can be rewritten as:

$$
A_{21}' (U_A X_A + A_{12}' X_S) + B_{12} X_B + (A_{22}' + B_{11}') X_S = Y_{A2} + Y_{B1}
$$

We see that the factor $X_A' = U_A X_A + A_{12}' X_S$ actually appears in both the local solution and the global equations, and this is what allows us to split the LU decomposition into smaller parts. We rewrite:

$$
(A_{22}' + B_{11}) X_S + B_{12} X_B = Y_{B1} + (Y_{A2} - A_{21}' X_A')
$$

After solving the global matrix for $X_S$ and $X_B$, the local matrix can continue with a backward substitution step as well.

$$
\begin{align*}
U_A X_A + A_{12}' X_S &= Y_{A1} \\
&\Downarrow \\
U_A X_A &= Y_{A1} - A_{12}' X_S
\end{align*}
$$

So in summary:

1. We load the local solver (*local*).
2. We perform partial LU decomposition on the local solver, making sure all connections to the outside are at the end of the local solver. This gives us $A_{12}'$, $A_{21}'$, $A_{22}'$ (*local*).
3. We perform forward substitution to find $X_A'$ (*local*).
4. We now load the global solver with $A_{22}'$ in the Y-matrix and $Y_{A2} - A_{21}' X_A'$ in the right-hand side vector (*global*).
5. We perform full LU decomposition on the global solver. This gives us $X_S$ and $X_B$ (*global*).
6. We finally copy the contributions by $X_S$ to the local solver, and complete the backward substitution steps to find $X_A$ (*local*).

As can be seen, most steps can be performed locally. This potentially allows many subcircuits to perform not only loading of the solver in parallel, but also to perform part of the LU decomposition in parallel. There is some associated overhead, so it only works well if $X_A$ is large and $X_S$ is small. In other words, if the subcircuit has many local nodes and branch currents, and the number of ports of the subcircuit is limited, this method of parallelization can improve performance.