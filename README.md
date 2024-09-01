# <img src="https://spicesharp.github.io/SpiceSharp/api/images/logo_full.svg" width="45px" /> Spice# (SpiceSharp)

Spice# is a Spice circuit simulator written in C#. The framework is made to be compatible with the original Berkeley Spice simulator, but bugs have been squashed and features can and will probably will be added.

## Documentation

You can find documentation at [https://spicesharp.github.io/SpiceSharp/](https://spicesharp.github.io/SpiceSharp/). There you can find a guide for **getting started**, as well as more information about:

- Supported types of analysis.
- The general structure of Spice#.
- A tutorial on how to implement your own *custom* model equations (prerequisite knowledge needed).
- An example of changing parameters during simulation.
- etc.

## Quickstart

Simulating a circuit is relatively straightforward. For example:

```csharp
using System;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            // Build the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new Resistor("R1", "in", "out", 1.0e3),
                new Resistor("R2", "out", "0", 2.0e3)
                );

            // Create a DC sweep and register to the event for exporting simulation data
            var dc = new DC("dc", "V1", 0.0, 5.0, 0.001);

            // Run the simulation
            foreach (int exportType in dc.Run(ckt))
            {
                Console.WriteLine(dc.GetVoltage("out"))
            }
        }
    }
}
```

Most standard Spice-components are available, and building your own custom components is also possible!

## Installation

Spice# is available as a **NuGet Package**.

[![NuGet Downloads](https://img.shields.io/nuget/dt/SpiceSharp?label=Spice%23)](https://www.nuget.org/packages/SpiceSharp/)

## Current build status

|    | Status |
|:---|-------:|
| Windows | ![Windows Tests](https://github.com/SpiceSharp/SpiceSharp/workflows/Windows%20Tests/badge.svg) |
| MacOS | ![MacOS Tests](https://github.com/SpiceSharp/SpiceSharp/workflows/MacOS%20Tests/badge.svg) |
| Linux/Ubuntu | ![Linux Tests](https://github.com/SpiceSharp/SpiceSharp/workflows/Linux%20Tests/badge.svg) |

## Aim of Spice#?

Spice# aims to be:

- A **Library** rather than a standalone piece of software like most simulators currently are.
- **Accessible** for both the amateur and advanced electronics enthusiast (and perhaps professional designer). In order to decrease the hurdle, a [Spice# parser](https://github.com/SpiceSharp/SpiceSharpParser) is also being developed. This also includes it being cross-platform (.NET and Mono).
- **Compatible** with the *original Spice 3f5* software (without the bugs). There's a reason why this has become the industry standard.
- **Customizable** with custom simulations, custom models, integration methods, solver, etc.
- **Performance**, but still completely managed code. Nobody wants a slow simulator.

## What Spice# is not

Having been implemented in the .NET framework does have some limitations:

- Unmanaged C/C++ code can often be optimized more than managed code.
- Spice# uses *Reflection* to give you a better experience. However if you decide to use reflection, you may feel some performance hit.
