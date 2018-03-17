# Getting started
In this section we will quickly go over everything needed to create a simple circuit and simulate it.

## Building the circuit
Let's start with a very simple circuit called a *resistive voltage divider*. The schematic looks as follows.

<p align="center"><img src="images/example01.svg" /></p>

[!code-csharp[Circuit](../../SpiceSharpTest/BasicExampleTests.cs#example01_build)]

## Running a DC simulation

[!code-csharp[Simulation](../../SpiceSharpTest/BasicExampleTests.cs#example01_simulate)]