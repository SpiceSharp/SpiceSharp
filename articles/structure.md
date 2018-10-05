# Structure

In this section we will discuss the basic structure of any simulation that is run. We will discuss:

- The data structure of Spice#.
- How any **[Simulation](xref:SpiceSharp.Simulations.Simulation)** use a **[Circuit](xref:SpiceSharp.Circuit)** object to run a simulation.
- How any **[Simulation](xref:SpiceSharp.Simulations.Simulation)** runs an analysis.

## Circuit description

The main container for storing your circuit will be the **[Circuit](xref:SpiceSharp.Circuit)** class. It contains one or more **[Entity](xref:SpiceSharp.Circuits.Entity)** objects which can be referenced by their name. Each **[Entity](xref:SpiceSharp.Circuits.Entity)** contains parameters, which can be accessed through the **[ParameterSets](xref:SpiceSharp.Circuits.Entity#SpiceSharp_Circuits_Entity_ParameterSets)** property. This is a collection of classes (all implementing **[ParameterSet](xref:SpiceSharp.ParameterSet)**) that can be searched by type.

For example, a **[Resistor](xref:SpiceSharp.Components.Resistor)** will typically contain one set of parameters, namely an object of the class  **[SpiceSharp.Components.ResistorBehaviors.BaseParameters](xref:SpiceSharp.Components.ResistorBehaviors.BaseParameters)**. This parameter set contains - among others - the **[Resistance](xref:SpiceSharp.Components.ResistorBehaviors.BaseParameters#SpiceSharp_Components_ResistorBehaviors_BaseParameters_Resistance)** of the resistor. So we can change the resistance of a **[Resistor](xref:SpiceSharp.Components.Resistor)** by writing:

[!code-csharp[Resistor](../SpiceSharpTest/BasicExampleTests.cs#example_structure_resistor)]

Alternatively, we can see that the same property is tagged with a **[ParameterNameAttribute](xref:SpiceSharp.Attributes.ParameterNameAttribute)** that gives the property the name "resistance", and a **[ParameterInfoAttribute](xref:SpiceSharp.Attributes.ParameterInfoAttribute)** that indicates that the property is a *principal* parameter. These two attributes allow us to edit the same property in two other ways. Respectively:

[!code-csharp[Resistor 2](../SpiceSharpTest/BasicExampleTests.cs#example_structure_resistor_2)]

Depending on the situation, parameter sets can be added for different types of simulations or situations.

## Simulation description

Simulations can be set up in a similar fashion. They also contain a property with parameter sets called **[Configurations](xref:SpiceSharp.Simulations.Simulation#SpiceSharp_Simulations_Simulation_Configurations)**. For example, a **[DC](xref:SpiceSharp.Simulations.DC)** simulation will use a **[DCConfiguration](xref:SpiceSharp.Simulations.DCConfiguration)** to determine the sweeps of the analysis. These can be accessed using the following code:

[!code-csharp[DC example](../SpiceSharpTest/BasicExampleTests.cs#example_structure_dc)]

The **[DC](xref:SpiceSharp.Simulations.DC)** class also implements **[BaseSimulation](xref:SpiceSharp.Simulations.BaseSimulation)** which also means it uses a **[BaseConfiguration](xref:SpiceSharp.Simulations.BaseConfiguration)** to have access to for example the tolerance on accepted solutions.

[!code-csharp[DC example 2](../SpiceSharpTest/BasicExampleTests.cs#example_structure_dc_2)]

## Running simulations

All simulations implement the **[Simulation](xref:SpiceSharp.Simulations.Simulation)** class. The whole act of analysis can be summarized in the following flowchart.

<p align="center"><img src="images/simulation_flow.svg" alt="Simulation flow" /></p>

The processes in yellow indicate events for which you can register.

### Setup

During setup the simulation will copy *all* data necessary for running the simulation. This makes the simulation fully self-supporting, allowing multiple simulations to run in parallel from the same circuit description.

In this method, the simulation will request two things from each **[Entity](xref:SpiceSharp.Circuits.Entity)**:
- The necessary **[Behavior](xref:SpiceSharp.Behaviors.Behavior)** objects to run the simulation. Behaviors describe how the entity behaves in a specific situation, and work in tandem with the simulation requesting it. If the entity cannot generate the requested behavior, it is ignored. All generated behaviors can be found in the **[EntityBehaviors](xref:SpiceSharp.Simulations.Simulation#SpiceSharp_Simulations_Simulation_EntityBehaviors)** property.
- All parameter sets of the entity are *cloned* to avoid issues when multithreading. All the cloned parameters can be found in the **[EntityParameters](xref:SpiceSharp.Simulations.Simulation#SpiceSharp_Simulations_Simulation_EntityParameters)** property.

Each behavior is set up as well and default parameters are calculated (eg. parameters that are derived from other parameters if they are not set by the user).

Another thing that happens during setup is creating all the *unknown variables* that will need to be solved. This is usually all node voltages in the circuit, but can also contain other types of variables. These are stored in the **[Variables](xref:SpiceSharp.Simulations.Simulation#SpiceSharp_Simulations_Simulation_Variables)** property.

### Execute

Execution of the simulation is entirely dependent on the type of simulation. After the simulation execution has finished, you have the option to *repeat* the simulation. This is by subscribing to the *AfterExecute* event, and changing the argument property **[Repeat](xref:SpiceSharp.Simulations.SimulationFlowEventArgs#SpiceSharp_Simulations_SimulationFlowEventArgs_Repeat)**.

### Unsetup/Destroy

In this phase the simulation will dispose of all the data previously allocated during setup.

<div class="pull-right">[Next: Entities, components and models](entities.md)</p>
