# SpiceSharp
SpiceSharp is a Spice circuit simulator written in C#. It uses Math.NET to solve matrix equations. The simulator currently includes AC, DC and transient simulations. The framework is made to resemble the original Berkeley Spice version, but some parts have been altered/improved to fit into the .NET framework.

I try to verify with other simulators using unit tests: ngSpice (PartSim), LTSpice (LTSpice XVII), SmartSpice (Gateway, Silvaco). This is not always easy, as each simulator makes other design choices and optimizations (LTSpice extended the diode model, Gateway adds extra GMIN conductances, etc.).

Please note that this project is in no case meant to compete with existing commercial Spice simulators. I wanted to know more about the  Spice simulator, and I wanted to be able to extend its functionality in useful ways (eg. automating simple designs, modeling custom components, etc.)

## Features
Supported simulations:
- DC sweeps
- AC analysis
- Transient simulation
Other simulations may be added later.

## Usage
The main project is called *SpiceSharp*. This project contains the framework for circuit simulation and has one dependency: Math.NET.
The project *Spice2SpiceSharp* contains a tool that I use and try to maintain to convert Spice models to the SpiceSharp framework. Note that after conversion, it is still necessary to intervene in some parts. But it does make the process a lot easier for large models.
The project *SpiceSharpParser* contains a netlist parser and expression parser for Spice netlists. You can pass it any stream where it will read components, models, subcircuits, simulation statements and more.

### SpiceSharp
Using SpiceSharp can be very easy. The *Circuit* class will be your main class. Any component, model or subcircuit that implements *ICircuitObject* can be added to do something when simulating your circuits.
The standard circuit components and models are in the namespace *SpiceSharp.Components*. Simulations are in *SpiceSharp.Simulations*.
```C#
// Create a resistive voltage divider
Circuit ckt = new Circuit();
ckt.Objects.Add(
  new Voltagesource("V1", "IN", "GND", 5.0),
  new Resistor("R1", "IN", "OUT", 1e3),
  new Resistor("R2", "OUT", "GND", 2e3)
);
```

A component can also implement the *IParameterized* (SpiceSharp.Parameters) interface. In this case, the class can then specify identifiers by which it wishes to expose the parameter, along with some extra information. For example, if we wish to specify an AC magnitude signal for the voltage source we created before, we can write:
```C#
IParameterized vsrc = (IParameterized)ckt.Objects["V1"];
vsrc.Set("acmag", 1.0);
```
This is because this parameter is exposed using attributes in the *Voltagesource*-class using:
```C#
[SpiceName("acmag"), SpiceInfo("A.C. Magnitude")]
public Parameter VSRCacMag { get; } = new Parameter();
```

Let's do a DC sweep:
```C#
DC dc = new DC("DC 1");
dc.Sweeps.Add(new DC.Sweep("V1", 0, 5, 0.1));
dc.OnExportSimulationData += (object sender, SimulationData data) =>
{
  Console.WriteLine(dc.Sweeps[0].CurrentValue + ": " + data.GetVoltage("OUT"));
};
ckt.Simulate(dc);
```
Each simulation implements *ISimulation* and will invoke *OnExportSimulationData* when a new point has be calculated. You can use this event to extract the voltages and currents that are of interest.

The models included in the main project are:
- Passive components: Resistor, Capacitance, Inductor, Mutual inductance
- Voltage sources and current sources: Independent, voltage-controlled, current-controlled
- Bipolar transistor (BJT)
- MOSFET: MOS1 (level 1), MOS2 (level 2), MOS3 (level 3)
- Diode (D)
- Switches: Voltage switch, Current switch
The simulations included in the main project are:
- AC simulation
- DC simulation
- Transient simulation
The waveforms for independent sources included in the main project are:
- Pulse
- Sine

### SpiceSharpParser
This project can import the circuits from text sources. The format is made to match the original Spice commands and uses a tool called CSharpCC (a port from JavaCC) to generate the lexer and parser.
The parser is extended with some useful features that you often find in more advanced/commercial Spice simulators:
- Subcircuits can be specified with their own parameters: .SUBCKT name A B PARAMS: a=1 b=2 c=3 can be called using X1 A B name a=1.5 b=1.6.
- Parameters can be specified using *.PARAM*
- Full expressions using parameters can be used
Using any non-literal value (ie. expressions) have to be enclosed in accolades "{" and "}". For example:
```
* Is allowed
R1 IN OUT {rscale*1k}

* Is not allowed, because r is a parameter so it should be replaced by {r}
R1 IN OUT r
```

### SpiceSharpBSIM
This project contains the BSIM models:
- BSIM1 (level = 4)
- BSIM2 (level = 5)
- BSIM3 (commonly level = 49, only latest version 3.3.0)
- BSIM4 (latest version 4.8.0)
