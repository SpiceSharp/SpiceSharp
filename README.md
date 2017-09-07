# SpiceSharp <img src="https://github.com/svenboulanger/SpiceSharp/blob/master/SpiceNetIcon.png?raw=true" width="45px" />
SpiceSharp is a Spice circuit simulator written in C#. It uses Math.NET to solve matrix equations. The simulator currently includes AC, DC and transient simulations. The framework is made to resemble the original Berkeley Spice version, but some parts have been altered/improved to fit into the .NET framework.

I try to verify with other simulators using unit tests: ngSpice (PartSim), LTSpice (LTSpice XVII), SmartSpice (Gateway, Silvaco). This is not always easy, as each simulator makes other design choices and optimizations (LTSpice extended the diode model, Gateway adds extra GMIN conductances, etc.).

Please note that this project is by no means meant to compete with existing commercial Spice simulators, although its performance is probably similar. I wanted to know more about the Spice simulator, and I wanted to be able to extend its functionality in useful ways (eg. automating simple designs, modeling custom components, etc.)

SpiceSharp is available as a **NuGet Package**.

[![NuGet Badge](https://buildstats.info/nuget/spicesharp)](https://www.nuget.org/packages/SpiceSharp/) SpiceSharp <br />
[![NuGet Badge](https://buildstats.info/nuget/spicesharpparser)](https://www.nuget.org/packages/SpiceSharpParser/) SpiceSharp Parser

[![Build status](https://ci.appveyor.com/api/projects/status/hhg89ejd795ykmvh?svg=true)](https://ci.appveyor.com/project/svenboulanger/spicesharp)

You can find the API documentation here:<br />
SpiceSharp: https://svenboulanger.github.io/SpiceSharp/coreapi/api/index.html<br />
SpiceSharp.Parser: https://svenboulanger.github.io/SpiceSharp/parserapi/api/index.html

## SpiceSharp
The basic usage is pretty easy. A `Circuit` object will hold all circuit objects, and can run a simulation. For example, doing a transient analysis of a simple RC-filter will look like this:

```C#
// Build the circuit
Circuit ckt = new Circuit();
ckt.Objects.Add(
    new Voltagesource("V1", "IN", "GND", new Pulse(0, 5, 1e-3, 1e-5, 1e-5, 1e-3, 2e-3)),
    new Resistor("R1", "IN", "OUT", 1e3),
    new Capacitor("C1", "OUT", "GND", 1e-6)
    );

// Simulation
Transient tran = new Transient("Tran 1", 1e-6, 20e-3);
tran.OnExportSimulationData += (object sender, SimulationData data) =>
    {
        double time = data.GetTime();
        double output = data.GetVoltage("OUT");
    };
ckt.Simulate(tran);
```

## SpiceSharp.Parser
An additional project has been published on NuGet that facilitates parsing Spice netlists. Parsing netlists is done using the `NetlistReader` class. For example:

```C#
string netlist = string.Join(Environment.NewLine,
    ".MODEL diomod D is=1e-14",
    "Vinput IN GND 0.0",
    "Rseries IN OUT {1k * 10}",
    "Dload OUT GND diomod",
    ".SAVE v(OUT)",
    ".DC Vinput -5 5 50m"
    );
NetlistReader nr = new NetlistReader();
MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(netlist));
nr.Parse(ms);
nr.Netlist.OnExportSimulationData += (object sender, SimulationData data) =>
{
    double inp = data.GetVoltage("in");
    double outp = nr.Netlist.Exports[0].Extract(data);
};
nr.Netlist.Simulate();
```

The parser features:
- A light-weight but fast expression parser. Put expressions between "{" and "}"
- An expandable library of readers for circuit components (R, C, L, D, ...) and control statements (.SAVE, .TRAN, ...)
- Subcircuits with parameters
