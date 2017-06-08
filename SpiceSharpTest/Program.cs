using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Circuit ckt = new Circuit();
            var vsrc = new Voltagesource("V1");
            vsrc.Connect("IN", "GND");
            vsrc.Set("dc", 5.0);
            ckt.Components.Add(vsrc);

            var res = new Resistor("R1");
            res.Connect("IN", "OUT");
            res.Set("resistance", 1e3);
            ckt.Components.Add(res);

            res = new Resistor("R2");
            res.Connect("OUT", "GND");
            res.Set("resistance", 1e3);
            ckt.Components.Add(res);

            ckt.Setup();
            Transient t = new Transient();
            foreach (var c in ckt.Components)
                c.Temperature(ckt);
            t.Op(ckt, 0, 100);

            Console.WriteLine("Result: " + ckt.State.Solution.ToString());
            Console.ReadKey();
        }
    }
}