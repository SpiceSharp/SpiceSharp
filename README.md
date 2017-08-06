# SpiceSharp
A Spice-based circuit simulator written in C#. It uses Math.NET to solve matrix equations.

The main project is SpiceSharp. It currently supports AC, DC and Transient simulations. It has the following models implemented:
* Voltage sources (Voltagesource, VoltageControlledVoltagesource, CurrentControlledVoltagesource)
* Current sources (Currentsource, VoltageControlledCurrentsource, CurrentControlledCurrentsource)
* Resistors (Resistor, ResistorModel)
* Capacitors (Capacitor, CapacitorModel)
* Inductors (Inductor, MutualInductance)
* Switches (VoltageSwitch, VoltageSwitchModel, CurrentSwitch, CurrentSwitchModel)
* BJT (Bipolar, BipolarModel)
* Diodes (Diode, DiodeModel)

The project SpiceSharpTransistors project contains additional transistor models:
* MOS1 (LEVEL 1)
* MOS2 (LEVEL 2)
* MOS3 (LEVEL 3)
* (MOS6, seemed to be incomplete)
* BSIM1
* BSIM2
* BSIM3 (3.3.0)
* BSIM4 (4.8.0)

The project Spice2SpiceSharp contains a tool that I use to convert Spice models to the SpiceSharp framework (C and C# are not that different when it comes to model calculations). This minimizes the error I make when porting the model to this framework. Some manual work is still necessary though, and errors can still sneak in. Use the models at your own risk.

The project SpiceSharpParser contains a netlist parser. It uses a tool CSharpCC (which is a port from JavaCC) to generate the parser. The file spicelang.cc contains the grammar. You can use the class NetlistReader to parse netlists from a stream.

Although many features have been copied or integrated from Spice 3f5, this is not an exact copy. It might behave slightly different from other Spice simulators (eg. timestep control for transient simulations).

## Example
```C#
using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest
{
    class Program
    {
        /// <summary>
        /// Output variables
        /// </summary>
        private static List<double> freq = new List<double>();
        private static List<double> output = new List<double>();

        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args">Input arguments</param>
        static void Main(string[] args)
        {
            // Allow conversion from Spice strings to doubles
            SpiceSharp.Parameters.SpiceMember.SpiceMemberConvert += SpiceSharp.Parameters.Converter.SpiceConvert;

            // Create a circuit object
            Circuit ckt = new Circuit();

            // Create all components
            ckt.Components.Add(
                new Voltagesource("V1", "IN", "GND", 0),
                new Resistor("R1", "IN", "OUT", "1k"),
                new Capacitor("C1", "OUT", "GND", "1u"));
            ckt.Components["V1"].Set("acmag", 1);
            
            // Run the simulation
            AC ac = new AC("AC1", "dec", 5, 1, "10meg");
            ac.ExportSimulationData += Ac_ExportSimulationData;
            ckt.Simulate(ac);

            // Show all the warnings
            Console.WriteLine("Warnings:");
            foreach (string msg in SpiceSharp.Diagnostics.CircuitWarning.Warnings)
                Console.WriteLine(msg);

            Console.ReadKey();
        }

        /// <summary>
        /// Export simulation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private static void Ac_ExportSimulationData(object sender, SimulationData data)
        {
            double f = data.GetFrequency();
            double a = data.GetDb("OUT");

            Console.WriteLine(f + " - db = " + a);
        }
    }
}
```

### Components
Components can be added and removed via the ckt.Components property. To set a parameter, you can use the Set() method. To ask a parameter, you can use the Ask() method. All components are in the SpiceSharp.Components namespace.
To get a list of all usable parameters, you can use the following code:
```C#
string[] names = Parameterized.GetParameterNames(ckt.Components["V1"]);
string description = Parameterized.GetParameterDescription(ckt.Components["V1"], "acmag");
Type valuetype = Parameterized.GetParameterType(ckt.Components["V1"], "acmag");
```

It is also possible to just access the variables directly to avoid resolving it at runtime. For example, the following code will have the same effect:
```C#
Resistor r = new Resistor("R1");
r.Set("resistance", "1k");
r.RESresist.Set(1.0e3); // RESresist of type Parameter<double>, which also keeps track whether or not the variable is set by the user
```

When a parameter object is not of the expected type, the SpiceMemberConvert event is called. The Convert.SpiceConvert method can be used as a default implementation that can parse basic Spice parameters such as "1uF", "1g", "1e9", etc. 
```C#
SpiceSharp.Parameters.SpiceMember.SpiceMemberConvert += SpiceSharp.Parameters.Converter.SpiceConvert;
```
If the value still hasn't been converted after running all events, there will be a final attempt to convert the value using the Convert.ToDouble()/ToInt32()/... method.

### Simulations
There are 3 simulations available: AC, DC and Transient. Their parameters can be set, asked or listed in the same way as components. Only the DC-simulation requires you to add the sweeps to the Sweep-property.
If you wish to extract data from the simulation, you need to add your method to the ExportSimulationData event. The method can then go on to extract simulation data.

The simplest way to extract data is to just use the SimulationData-class to fetch the data for you (eg. using data.GetVoltage("OUT") gets the voltage at node "OUT"). Or you can also ask the data from the component in some cases. For example, the current through a voltage source can be asked:
```C#
private static void Ac_ExportSimulationData(object sender, SimulationData data)
{
    double i = data.Circuit.Components["V1"].Ask("i", data.Circuit);
}
```
Note that the circuit will contain all simulation data, so the circuit needs to be passed to the Ask-method in order to extract the current.

### Parsing netlists
The netlist parser makes it easier to read netlists. The following illustrates reading a netlist file "test.net".
```C#
NetlistReader parser = new NetlistReader();
SpiceSharp.Parameters.SpiceMember.SpiceMemberConvert += SpiceSharp.Parameters.Converter.SpiceConvert;
parser.Parse("test.net");
```
Note that the second line allows reading values of the type "10u" instead of only "10e-6".
The NetlistReader class contains a Netlist object that contains all the IReader interfaces for parsing the netlist.
- ComponentReaders: A list of component readers (eg. "DiodeReader()" reads "D1_b IN OUT dmod1").
- ControlReaders: A list of control statement readers (eg. "TransientReader()" reads ".tran 1n 10p").
The parsed results are stored in the Circuit and Simulations properties.

There is a special ControlReaders class called ModelReader. This class reads ".model" statements and passes the name and parameters to another list of readers.
