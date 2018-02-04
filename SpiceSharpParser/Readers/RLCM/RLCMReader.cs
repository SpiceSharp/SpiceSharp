using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads <see cref="Resistor"/>, <see cref="Capacitor"/>, <see cref="Inductor"/> and <see cref="MutualInductance"/> components.
    /// </summary>
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
        protected override Entity Generate(string type, Identifier name, List<Token> parameters, Netlist netlist)
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
        protected Entity GenerateCap(Identifier name, List<Token> parameters, Netlist netlist)
        {
            Capacitor cap = new Capacitor(name);
            cap.ReadNodes(netlist.Path, parameters);

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
                        cap.SetModel(netlist.Path.FindModel<CapacitorModel>(netlist.Circuit.Objects, new Identifier(parameters[2].image)));
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
        protected Entity GenerateInd(Identifier name, List<Token> parameters, Netlist netlist)
        {
            Inductor ind = new Inductor(name);
            ind.ReadNodes(netlist.Path, parameters);

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
        protected Entity GenerateMut(Identifier name, List<Token> parameters, Netlist netlist)
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
            mut.MUTind1 = new Identifier(parameters[0].image);
            if (!ReaderExtension.IsName(parameters[1]))
                throw new ParseException(parameters[1], "Component name expected");
            mut.MUTind2 = new Identifier(parameters[1].image);
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
        protected Entity GenerateRes(Identifier name, List<Token> parameters, Netlist netlist)
        {
            Resistor res = new Resistor(name);
            res.ReadNodes(netlist.Path, parameters);

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
