# Getting started
In this section we will quickly go over everything needed to create a simple circuit and simulate it.

## Building the circuit
Let's start with a very simple circuit called a *resistive voltage divider*. The schematic looks as follows.

<p align="center"><img src="images/example01.svg" /></p>

Creating this circuit is done using the `Circuit`-class. This is a container of multiple entities, such as voltage sources and resistors. The `Circuit`-class is defined in the namespace `SpiceSharp`, while all default components are in the namespace `SpiceSharp.Components`.

[!code-csharp[Circuit](../SpiceSharpTest/BasicExampleTests.cs#example01_build)]

## Running a DC simulation

A DC simulation will (by default) sweep a voltage or current source value. the result is a DC transfer curve in function of the swept parameter.

We will sweep the input voltage source from -1V to 1V in steps of 100mV.

[!code-csharp[Simulation](../SpiceSharpTest/BasicExampleTests.cs#example01_simulate)]