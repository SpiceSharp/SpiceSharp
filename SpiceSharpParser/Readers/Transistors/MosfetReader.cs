using System;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Read Mosfet transistor components.
    /// Mosfets are generated based on their model class. Default generators are:
    /// - <see cref="MOS1Model"/> will generate <see cref="MOS1"/>
    /// - <see cref="MOS2Model"/> will generate <see cref="MOS2"/>
    /// - <see cref="MOS3Model"/> will generate <see cref="MOS3"/>
    /// </summary>
    public class MosfetReader : ComponentReader
    {
        /// <summary>
        /// Generate a mosfet instance based on a model.
        /// The generator is passed the arguments name and model.
        /// </summary>
        public Dictionary<Type, Func<CircuitIdentifier, ICircuitObject, ICircuitComponent>> Mosfets { get; } = new Dictionary<Type, Func<CircuitIdentifier, ICircuitObject, ICircuitComponent>>();

        /// <summary>
        /// Constructor
        /// </summary>
        public MosfetReader()
            : base("m")
        {
            // MOS1
            Mosfets.Add(typeof(MOS1Model), (CircuitIdentifier name, ICircuitObject model) =>
            {
                var m = new MOS1(name);
                m.SetModel((MOS1Model)model);
                return m;
            });

            // MOS2
            Mosfets.Add(typeof(MOS2Model), (CircuitIdentifier name, ICircuitObject model) =>
            {
                var m = new MOS2(name);
                m.SetModel((MOS2Model)model);
                return m;
            });

            // MOS3
            Mosfets.Add(typeof(MOS3Model), (CircuitIdentifier name, ICircuitObject model) =>
            {
                var m = new MOS3(name);
                m.SetModel((MOS3Model)model);
                return m;
            });
        }
        
        /// <summary>
        /// Generate the mosfet instance
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override ICircuitObject Generate(string type, CircuitIdentifier name, List<Token> parameters, Netlist netlist)
        {
            // Errors
            switch (parameters.Count)
            {
                case 0: throw new ParseException($"Node expected for component {name}");
                case 1:
                case 2:
                case 3: throw new ParseException(parameters[parameters.Count - 1], "Node expected", false);
                case 4: throw new ParseException(parameters[3], "Model name expected");
            }

            // Get the model and generate a component for it
            ICircuitObject model = netlist.Path.FindModel<ICircuitObject>(netlist.Circuit.Objects, new CircuitIdentifier(parameters[4].image));
            ICircuitComponent mosfet = null;
            if (Mosfets.ContainsKey(model.GetType()))
                mosfet = Mosfets[model.GetType()].Invoke(name, model);
            else
                throw new ParseException(parameters[4], "Invalid model");

            // The rest is all just parameters
            mosfet.ReadNodes(netlist.Path, parameters);
            netlist.ReadParameters((IParameterized)mosfet, parameters, 4);
            return mosfet;
        }
    }
}
