# Base behaviors

Most simulations start out in the same way, implemented by the **[BaseSimulation](xref:SpiceSharp.Simulations.BaseSimulation)**:
1. During setup:
  - A **[BaseSimulationState](xref:SpiceSharp.Simulations.BaseSimulationState)** is created for solving equations with real numbers.
  - The **behaviors** needed for execution are set up. This includes allocating space in the Y-matrix and RHS-vector of the simulation state.
2. During execution:
  - **Temperature** dependent calculations are executed using the list of  **[ITemperatureBehavior](xref:SpiceSharp.Behaviors.ITemperatureBehavior)** objects created during setup.
  - The **operating point (OP)** is calculated using the list of  **[IBiasingBehavior](xref:SpiceSharp.Behaviors.IBiasingBehavior)** objects created during setup. The operating point assumes that the circuit is static (does not change over time). The circuit *bias* is numerically found by *iteratively* converging to a solution.
  - After finding the biasing point of the circuit, the rest of the simulation is executed. This can use the same or different **[SimulationState](xref:SpiceSharp.Simulations.SimulationState)** objects managed by the simulation.
3. During unsetup:
  - Remove allocated objects during setup of all behaviors and the simulation.

In this section, we will discuss how we can create a custom component and model that works for any simulation based on the **[BaseSimulation](xref:SpiceSharp.Simulations.BaseSimulation)**.

## The custom component - a nonlinear resistor

Let us borrow the same convention as a regular resistor.

<p align="center"><img width="100px" src="images/example_circuit_mna_res.svg" alt="Resistor definition" /></p>

But this time our custom resistor does *not* follow Ohm's law. Let us say we managed to model our resistor using the following relationship:

<p align="center"><img src="https://latex.codecogs.com/svg.latex?v_R&space;=&space;a\cdot&space;(i_R)^b" alt="v_R = a*(i_R)^b" /></p>

Our entity has 2 parameters, **a** and **b**, so will create a parameter set that can be used to describe our nonlinear behavior.

[!code-csharp[Base parameters](../../SpiceSharpTest/Examples/CustomResistor/BaseParameters.cs)]

### The biasing behavior

The biasing behavior will load the Y-matrix and RHS-vector according to the equation of our resistor. Similar to the previous chapter about [Modified Nodal Analysis](modified_nodal_analysis.md), we first calculate the contributions analytically.

The current ![i_R](https://latex.codecogs.com/svg.latex?\inline&space;i_R) flows out of node A and into node B, so we find that

<p align="center"><img src="https://latex.codecogs.com/svg.latex?\begin{align*}&space;v_R&=v_A-v_B\\&space;f_A(...,v_A,...,v_B,...)&=&plus;i_R=\left(\frac{v_R}{a}\right)^{(1/b)}\\&space;f_B(...,v_A,...,v_B,...)&=-i_R=-\left(\frac{v_R}{a}\right)^{(1/b)}&space;\end{align*}" /></p>

We calculate from this equation the contributions to the Y-matrix:

<img src="https://latex.codecogs.com/svg.latex?\begin{align*}&space;Y_{A,A}&=\frac{\partial&space;f_A}{\partial&space;v_A}=\frac{1}{a}\left(\frac{v_R^{(k)}}{a}\right)^{\frac{1}{b}-1}&=&plus;g\\&space;Y_{A,B}&=\frac{\partial&space;f_A}{\partial&space;v_B}=-\frac{1}{a}\left(\frac{v_R^{(k)}}{a}\right)^{\frac{1}{b}-1}&=-g\\&space;Y_{B,A}&=\frac{\partial&space;f_B}{\partial&space;v_A}=-\frac{1}{a}\left(\frac{v_R^{(k)}}{a}\right)^{\frac{1}{b}-1}&=-g\\&space;Y_{B,B}&=\frac{\partial&space;f_B}{\partial&space;v_B}=\frac{1}{a}\left(\frac{v_R^{(k)}}{a}\right)^{\frac{1}{b}-1}&=&plus;g&space;\end{align*}" />

And the contributions to the RHS-vector:

<img src="https://latex.codecogs.com/svg.latex?\begin{align*}&space;RHS_A&space;&=&space;&plus;\left((i_R)^{(k)}-g\cdot&space;v_R^{(k)}\right)\\&space;RHS_B&space;&=&space;-\left((i_R)^{(k)}-g\cdot&space;v_R^{(k)}\right)&space;\end{align*}" />

We now have everything to create our biasing behavior.

[!code-csharp[Load behavior](../../SpiceSharpTest/Examples/CustomResistor/BiasingBehavior.cs)]

Our behavior implements **[IBiasingBehavior](xref:SpiceSharp.Behaviors.IBiasingBehavior)**, which describes the contract for any behavior that can apply biasing logic.

In the *Bind* method, we can cache any information that we would find necessary for quickly calculating Y-matrix and Rhs-vector contributions. The base *Bind* method will store a reference to the simulation that can be used when loading the Y-matrix and/or RHS-vector. We can also extract the pins by casting the **[BindingContext](xref:SpiceSharp.Behaviors.BindingContext)** to the **[ComponentBindingContext](xref:SpiceSharp.Components.ComponentBindingContext)** class and querying for the pins of the component. In this method, the behavior also gets the chance to allocate elements in the Y-matrix and RHS vector (ie. ![elements](https://latex.codecogs.com/svg.latex?\inline&space;Y_{A,A},&space;Y_{A,B},&space;Y_{B,A},&space;Y_{B,B},&space;RHS_A,&space;RHS_B)).

In the *Unbind* method we simply remove any references to allow the garbage collector to clean up.

Finally we have the method called *Load*. Usually the behavior will in this method:
1. Find out the current iteration solution.
2. Calculate currents and derivatives.
  - This is usually optimized for speed, as this method is called the most - for every iteration!
  - In our example, we want the resistor to behave symmetrical. This is why we first track if the voltage is negative, and then take the absolute value.
  - We also make sure to avoid any situations where the simulator might get into trouble. For example, if the resistor behaves as an open circuit (`g=0`), we can inadvertently create floating nodes, which cannot be solved by the simulator. Hence, we add a very small conductance in this case.
3. We finally *add* or *subtract* the contributions to the Y-matrix and RHS-vector elements.

### The Component definition

All that is left is bringing it all together in a **[Component](xref:SpiceSharp.Components.Component)** to be able to pass it to a simulator.

[!code-csharp[Component definition](../../SpiceSharpTest/Examples/CustomResistor/NonlinearResistor.cs)]

We add an instance of our **BaseParameters** created earlier, and we provide a factory for our **BiasingBehavior**. When the simulation now asks the component for an **[IBiasingBehavior](xref:SpiceSharp.Behaviors.IBiasingBehavior)**, it will automatically create a new **BiasingBehavior** that can be used by the simulation.

### Using the component in a circuit

We can now plot the I-V curve using a simple **[DC](xref:SpiceSharp.Simulations.DC)** simulation. For example:

[!code-csharp[Nonlinear resistor DC](../../SpiceSharpTest/Examples/CustomResistor/NonlinearResistorTests.cs#example_customcomponent_nonlinearresistor_test)]

The resulting waveform is as expected:

<p align="center"><img src="images/example_custommodel_nlres_graph.svg" alt="I-V curve" /></p>

<div class="pull-left">[Previous: Modified Nodal Analysis](modified_nodal_analysis.md)</div> <div class="pull-right">[Next: Changing parameters during simulation](../changing_parameters/changing_parameters_during_simulation.md)</div>
