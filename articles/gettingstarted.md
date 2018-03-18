# Getting started
In this section we will quickly go over everything needed to create a simple circuit and simulate it.

## Installation

Installing can be done by using NuGet.

## Building the circuit
Let's start with a very simple circuit called a *resistive voltage divider*. The schematic looks as follows.

<p align="center"><img src="images/example01.svg" /></p>

The output voltage of this circuit is 2/3 times the input voltage.

Creating this circuit is done using the [Circuit](xref:SpiceSharp.Circuit)-class. This is a container of multiple entities, such as voltage sources and resistors. The [Circuit](xref:SpiceSharp.Circuit)-class is defined in the namespace @SpiceSharp, while all default components are in the namespace @SpiceSharp.Components.

[!code-csharp[Circuit](../SpiceSharpTest/BasicExampleTests.cs#example01_build)]

## Running a DC analysis

A [DC](xref:SpiceSharp.Simulations.DC) simulation will (by default) sweep a voltage or current source value. The result is a DC transfer curve in function of the swept parameter.

We will sweep the input voltage source from -1V to 1V in steps of 200mV.

[!code-csharp[Simulation](../SpiceSharpTest/BasicExampleTests.cs#example01_simulate)]

The output will yield as expected:

```
-1 V : -0.667 V
-0.8 V : -0.533 V
-0.6 V : -0.4 V
-0.4 V : -0.267 V
-0.2 V : -0.133 V
0 V : 0 V
0.2 V : 0.133 V
0.4 V : 0.267 V
0.6 V : 0.4 V
0.8 V : 0.533 V
1 V : 0.667 V
```

## Using exports

Using [Exports](xref:SpiceSharp.Simulations.Export`1) gives faster and more access to circuit properties. These exports also allow easier access to properties of components. For example, we could be interested in the current through voltage source V1. In which case we can some exports as follows:

[!code-csharp[Simulation](../SpiceSharpTest/BasicExampleTests.cs#example01_simulate2)]

Yielding the output:

```
-1 V : -0.667 V. 3.33E-05 A
-0.8 V : -0.533 V. 2.67E-05 A
-0.6 V : -0.4 V. 2E-05 A
-0.4 V : -0.267 V. 1.33E-05 A
-0.2 V : -0.133 V. 6.67E-06 A
0 V : 0 V. 0 A
0.2 V : 0.133 V. -6.67E-06 A
0.4 V : 0.267 V. -1.33E-05 A
0.6 V : 0.4 V. -2E-05 A
0.8 V : 0.533 V. -2.67E-05 A
1 V : 0.667 V. -3.33E-05 A
```