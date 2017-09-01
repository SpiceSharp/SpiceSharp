using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    public class RLCMReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RLCMReader()
            : base("r;l;c;k")
        {
        }

        /// <summary>
        /// Generate the resistor, inductor, capacitor or mutual inductance
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override ICircuitObject Generate(string type, string name, List<Token> parameters, Netlist netlist)
        {
            switch (type)
            {
                case "r": return GenerateRes(name, parameters, netlist);
                case "l": return GenerateInd(name, parameters, netlist);
                case "c": return GenerateCap(name, parameters, netlist);
                case "k": return GenerateMut(name, parameters, netlist);
            }
            return null;
        }

        /// <summary>
        /// Generate a capacitor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected ICircuitObject GenerateCap(string name, List<Token> parameters, Netlist netlist)
        {
            Capacitor cap = new Capacitor(name);
            cap.ReadNodes(parameters, 2);

            // Search for a parameter IC, which is common for both types of capacitors
            for (int i = 3; i < parameters.Count; i++)
            {
                if (parameters[i].kind == ASSIGNMENT)
                {
                    AssignmentToken at = parameters[i] as AssignmentToken;
                    if (at.Name.image.ToLower() == "ic")
                    {
                        double ic = netlist.ParseDouble(at.Value);
                        cap.CAPinitCond.Set(ic);
                        parameters.RemoveAt(i);
                        break;
                    }
                }
            }

            // The rest is just dependent on the number of parameters
            if (parameters.Count == 3)
                cap.CAPcapac.Set(netlist.ParseDouble(parameters[2]));
            else
            {
                cap.SetModel(netlist.FindModel<CapacitorModel>(parameters[2]));
                switch (parameters[2].kind)
                {
                    case WORD:
                    case IDENTIFIER:
                        cap.SetModel(netlist.Path.FindModel<CapacitorModel>(parameters[2].image.ToLower()));
                        break;
                    default:
                        throw new ParseException(parameters[2], "Model name expected");
                }
                netlist.ReadParameters(cap, parameters, 2);
                if (!cap.CAPlength.Given)
                    throw new ParseException(parameters[1], "L needs to be specified", false);
            }

            return cap;
        }

        /// <summary>
        /// Generate an inductor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected ICircuitObject GenerateInd(string name, List<Token> parameters, Netlist netlist)
        {
            Inductor ind = new Inductor(name);
            ind.ReadNodes(parameters, 2);

            // Read the value
            if (parameters.Count < 3)
                throw new ParseException(parameters[1], "Inductance expected", false);
            ind.INDinduct.Set(netlist.ParseDouble(parameters[2]));

            // Read initial conditions
            netlist.ReadParameters(ind, parameters, 3);
            return ind;
        }

        /// <summary>
        /// Generate a mutual inductance
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected ICircuitObject GenerateMut(string name, List<Token> parameters, Netlist netlist)
        {
            MutualInductance mut = new MutualInductance(name);
            switch (parameters.Count)
            {
                case 0: throw new ParseException($"Inductor name expected for mutual inductance \"{name}\"");
                case 1: throw new ParseException(parameters[0], "Inductor name expected", false);
                case 2: throw new ParseException(parameters[1], "Coupling factor expected", false);
            }

            // Read two inductors
            if (!ReaderExtension.IsName(parameters[0]))
                throw new ParseException(parameters[0], "Component name expected");
            mut.MUTind1 = parameters[0].image.ToLower();
            if (!ReaderExtension.IsName(parameters[1]))
                throw new ParseException(parameters[1], "Component name expected");
            mut.MUTind2 = parameters[1].image.ToLower();
            mut.MUTcoupling.Set(netlist.ParseDouble(parameters[2]));
            return mut;
        }

        /// <summary>
        /// Generate a resistor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected ICircuitObject GenerateRes(string name, List<Token> parameters, Netlist netlist)
        {
            Resistor res = new Resistor(name);
            res.ReadNodes(parameters, 2);

            // We have two possible formats:
            // Normal: RXXXXXXX N1 N2 VALUE
            if (parameters.Count == 3)
                res.RESresist.Set(netlist.ParseDouble(parameters[2]));
            else
            {
                // Read the model
                res.SetModel(netlist.FindModel<ResistorModel>(parameters[2]));
                netlist.ReadParameters(res, parameters, 3);
                if (!res.RESlength.Given)
                    throw new ParseException(parameters[parameters.Count - 1], "L needs to be specified", false);
            }
            return res;
        }
    }
}
