# Getting started
In this section we will try to quickly go over everything you need to create a simple circuit and simulate it using Spice#.

## Installation

The easiest way to install Spice# is by installing the NuGet package Spice#.

[![NuGet Badge](https://buildstats.info/nuget/spicesharp)](https://www.nuget.org/packages/SpiceSharp/)

You can also **clone** the repository directly. However, while you get the latest features and bug fixes, the documentation might not be up to date!

|    | Status |
|:---|-------:|
| Windows | ![Windows Tests](https://github.com/SpiceSharp/SpiceSharp/workflows/Windows%20Tests/badge.svg) |
| MacOS | ![MacOS Tests](https://github.com/SpiceSharp/SpiceSharp/workflows/MacOS%20Tests/badge.svg) |
| Linux/Ubuntu | ![Linux Tests](https://github.com/SpiceSharp/SpiceSharp/workflows/Linux%20Tests/badge.svg) |

## Building a circuit
Let's start with a very simple circuit known as a *resistive voltage divider*. The schematic looks as follows.

<p align="center"><img src="images/example01.svg" width="256px" /></p>

The output voltage of this circuit is 2/3 times the input voltage for those wondering.

The components are stored in a **[Circuit](xref:SpiceSharp.Circuit)**. This is a container for so-called entities (**[IEntity](xref:SpiceSharp.Entities.IEntity)**), which is the term for anything that can affect simulations. The **[Circuit](xref:SpiceSharp.Circuit)** is defined in the namespace *[SpiceSharp](xref:SpiceSharp)*, while all default components are typically specified in the namespace *[SpiceSharp.Components](xref:SpiceSharp.Components)*.

[!code-csharp[Circuit](../../SpiceSharpTest/BasicExampleTests.cs#example01_build)]

## Running a DC analysis on the circuit

A **[DC](xref:SpiceSharp.Simulations.DC)** simulation will sweep a voltage or current source value (or anything else in Spice#). The result is a transfer curve in function of the swept parameter.

We will sweep the input voltage source from -1V to 1V in steps of 200mV.

[!code-csharp[Simulation](../../SpiceSharpTest/BasicExampleTests.cs#example01_simulate)]

Access to simulation output data is usually achieved by using extension methods. These extension methods contain some code to access the most common aspects of the simulation states (like voltages and currents).

The output will show:

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

Using exports allows for faster access to voltages, currents, circuit properties, etc. compared to using the extension methods. For example, we could be interested in the current through voltage source V1. In which case we can define some exports like this:

[!code-csharp[Simulation](../../SpiceSharpTest/BasicExampleTests.cs#example01_simulate2)]

This will lead to the result:

1. `input = -1` (V), `output = -0.667` (V), `current = 3.33e-05` (A)
2. `input = -0.8` (V), `output = -0.533` (V), `current = 2.67e-05` (A)
3. `input = -0.6` (V), `output = -0.4` (V), `current = 2e-05` (A)
4. `input = -0.4` (V), `output = -0.267` (V), `current = 1.33e-05` (A)
5. `input = -0.2` (V), `output = -0.133` (V), `current = 6.67e-06` (A)
6. `input = 0` (V), `output = 0` (V), `current = 0` (A)
7. `input = 0.2` (V), `output = 0.133` (V), `current = -6.67e-06` (A)
8. `input = 0.4` (V), `output = 0.267` (V), `current = -1.33e-05` (A)
9. `input = 0.6` (V), `output = 0.4` (V), `current = -2e-05` (A)
10. `input = 0.8` (V), `output = 0.533` (V), `current = -2.67e-05` (A)
11. `input = 1` (V), `output = 0.667` (V), `current = -3.33e-05` (A)

