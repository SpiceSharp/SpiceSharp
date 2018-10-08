# Base behaviors

Most simulations start out in the same way:
1. During setup
  - The **behaviors** needed for execution are set up.
  - A **[BaseSimulationState](xref:SpiceSharp.Simulations.BaseSimulationState)** is created for solving equations with real numbers.
  - When the behaviors are created, the unknowns/variables are also allocated.
2. During execution
  - **Temperature** dependent calculations are executed using the list of  **[BaseTemperatureBehavior](xref:SpiceSharp.Behaviors.BaseTemperatureBehavior)** objects created during setup.
  - The **operating point (OP)** is calculated using the list of  **[BaseLoadBehavior](xref:SpiceSharp.Behaviors.BaseLoadBehavior)** objects created during setup. This step will need the simulation state using real numbers.
  - The rest of the simulation is executed.
    - Often involves *iterating* to a solution, which also uses the same **[BaseLoadBehavior](xref:SpiceSharp.Behaviors.BaseLoadBehavior)** objects.
3. During unsetup
  - Remove allocated objects during setup.

This means that (usually) the main things needed for *any* component, is
- a **[Component](xref:SpiceSharp.Components.Component)** and optionally a **[Model](xref:SpiceSharp.Components.Model)** that can be stored and connected in a **[Circuit](xref:SpiceSharp.Circuit)**.
- a **[TemperatureBehavior](xref:SpiceSharp.Behaviors.BaseTemperatureBehavior)** that describes what needs to be done when the *temperature* changes. Both the component and the model can optionally implement it.
- a **[BaseLoadBehavior](xref:SpiceSharp.Behaviors.BaseLoadBehavior)**, that describes how the component *loads* the simulation state, as seen in the previous chapter about [Modified Nodal Analysis](modified_nodal_analysis.md). Both the component and the model can optionally implement it.

In this section, we will discuss how we can create a custom component and model.

## The custom component - a nonlinear resistor

Let us borrow the same convention as a regular resistor.

<p align="center"><img width="100px" src="images/example_circuit_mna_res.svg" alt="Resistor definition" /></p>

But this time the resistor does *not* follow Ohm's law. Let us say we managed to model our resistor using the following relationship:

<p align="center"><img src="https://latex.codecogs.com/svg.latex?v_R&space;=&space;a\cdot&space;(i_R)^b" alt="v_R = a*(i_R)^b" /></p>

Our entity has 2 parameters, a and b. So will create some base parameters which can be used to describe our nonlinear behavior.

[!code-csharp[Base parameters](../../SpiceSharpTest/Examples/CustomResistor/BaseParameters.cs)]

### The load behavior

The load behavior will load the Y-matrix and RHS-vector according to the equation of our resistor. Similar to the previous chapter about [Modified Nodal Analysis](modified_nodal_analysis.md), we calculate the contributions.

The current ![i_R](https://latex.codecogs.com/svg.latex?\inline&space;i_R) flows out of node A and into node B, so we find that

<p align="center"><img src="https://latex.codecogs.com/svg.latex?\begin{align*}&space;v_R&=v_A-v_B\\&space;f_A(...,v_A,...,v_B,...)&=&plus;i_R=\left(\frac{v_R}{a}\right)^{(1/b)}\\&space;f_B(...,v_A,...,v_B,...)&=-i_R=-\left(\frac{v_R}{a}\right)^{(1/b)}&space;\end{align*}" /></p>

We calculate the contribution to the Y-matrix:

<img src="https://latex.codecogs.com/svg.latex?\begin{align*}&space;Y_{A,A}&=\frac{\partial&space;f_A}{\partial&space;v_A}=\frac{1}{a}\left(\frac{v_R^{(k)}}{a}\right)^{\frac{1}{b}-1}&=&plus;g\\&space;Y_{A,B}&=\frac{\partial&space;f_A}{\partial&space;v_B}=-\frac{1}{a}\left(\frac{v_R^{(k)}}{a}\right)^{\frac{1}{b}-1}&=-g\\&space;Y_{B,A}&=\frac{\partial&space;f_B}{\partial&space;v_A}=-\frac{1}{a}\left(\frac{v_R^{(k)}}{a}\right)^{\frac{1}{b}-1}&=-g\\&space;Y_{B,B}&=\frac{\partial&space;f_B}{\partial&space;v_B}=\frac{1}{a}\left(\frac{v_R^{(k)}}{a}\right)^{\frac{1}{b}-1}&=&plus;g&space;\end{align*}" />

And the contribution to the RHS-matrix, which is simply

<img src="https://latex.codecogs.com/svg.latex?\begin{align*}&space;RHS_A&space;&=&space;&plus;\left((i_R)^{(k)}-g\cdot&space;v_R^{(k)}\right)\\&space;RHS_B&space;&=&space;-\left((i_R)^{(k)}-g\cdot&space;v_R^{(k)}\right)&space;\end{align*}" />

We now have everything to create our loading behavior. The load behavior is shown below

[!code-csharp[Load behavior](../../SpiceSharpTest/Examples/CustomResistor/LoadBehavior.cs)]

Our behavior implements
- **[BaseLoadBehavior](xref:SpiceSharp.Behaviors.BaseLoadBehavior)**, which is the layout for any loading behavior.
- Since our behavior also needs to be updated with the connected nodes of the component in the circuit, we also implement **[IConnectedBehavior](xref:SpiceSharp.Components.IConnectedBehavior)**. The entity will pass its connection information through the *Connect* method when the behavior is created.

In the *Setup* method, the necessary parameters and behaviors can be requested in order to be cached. Since behaviors are created in the order they are called, it is impossible to request a temperature behavior in our load behavior. But it *is* possible for the load behavior to request the temperature behavior (if we had implemented it).

In the *Unsetup* method we simply remove the reference to our base parameters to allow the garbage collector to clean up.

After the behavior has been set up, the *GetEquationPointers* method is called. In this method, the behavior gets the chance to allocate elements in the Y-matrix and RHS vector (ie. ![elements](https://latex.codecogs.com/svg.latex?\inline&space;Y_{A,A},&space;Y_{A,B},&space;Y_{B,A},&space;Y_{B,B},&space;RHS_A,&space;RHS_B)).

Finally we have the method called *Load*. Usually the behavior will in this method
1. Find out the current iteration solution.
2. Calculate currents and derivatives.
  - This is usually optimized for speed, as this method is called the most!
  - In our example, we want the resistor to behave symmetrical. This is why we first track if the voltage is negative, and then take the absolute value.
3. We finally *add* or *subtract* the contributions to the Y-matrix and RHS-vector.

### The Component definition

All that is left is bringing it all together in a **[Component](xref:SpiceSharp.Components.Component)**.

[!code-csharp[Component definition](../../SpiceSharpTest/Examples/CustomResistor/NonlinearResistor.cs)]

We add an instance of our **BaseParameters** created earlier, and we provide a factory for our **LoadBehavior**. When the simulation now asks the component for a **[BaseLoadBehavior](xref:SpiceSharp.Behaviors.BaseLoadBehavior)**, it will automatically create a new **LoadBehavior** that can be used by the simulation.

### Using the component in a circuit

We can now plot the I-V curve using a simple **[DC](xref:SpiceSharp.Simulations.DC)** simulation. For example:

[!code-csharp[Nonlinear resistor DC](../../SpiceSharpTest/Examples/CustomResistor/NonlinearResistorTests.cs#example_customcomponent_nonlinearresistor_test)]

The resulting waveform is as expected:

<p align="center"><img src="images/example_custommodel_nlres_graph.svg" alt="I-V curve" /></p>

<div class="pull-left">[Previous: Modified Nodal Analysis](modified_nodal_analysis.md)</div>
