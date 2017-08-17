using System;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parameters;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Can read Mosfets
    /// </summary>
    public class MosfetReader : ComponentReader
    {
        /// <summary>
        /// Get the mosfet for a certain model
        /// </summary>
        public Dictionary<Type, Func<string, ICircuitObject, ICircuitComponent>> Mosfets { get; } = new Dictionary<Type, Func<string, ICircuitObject, ICircuitComponent>>();

        /// <summary>
        /// Constructor
        /// </summary>
        public MosfetReader()
            : base("m")
        {
            Mosfets.Add(typeof(MOS1Model), (string name, ICircuitObject model) =>
            {
                var m = new MOS1(name);
                m.SetModel((MOS1Model)model);
                return m;
            });
            Mosfets.Add(typeof(MOS2Model), (string name, ICircuitObject model) =>
            {
                var m = new MOS2(name);
                m.SetModel((MOS2Model)model);
                return m;
            });
            Mosfets.Add(typeof(MOS3Model), (string name, ICircuitObject model) =>
            {
                var m = new MOS3(name);
                m.SetModel((MOS3Model)model);
                return m;
            });
        }
        
        /// <summary>
        /// Generate the mosfet instance
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <param name="netlist"></param>
        /// <returns></returns>
        protected override ICircuitObject Generate(string type, string name, List<Token> parameters, Netlist netlist)
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
            ICircuitObject model = netlist.Path.FindModel<ICircuitObject>(parameters[4].image.ToLower());
            ICircuitComponent mosfet = null;
            if (Mosfets.ContainsKey(model.GetType()))
                mosfet = Mosfets[model.GetType()].Invoke(name, model);
            else
                throw new ParseException(parameters[4], "Invalid model");

            // The rest is all just parameters
            mosfet.ReadNodes(parameters, 4);
            netlist.ReadParameters((IParameterized)mosfet, parameters, 4);
            return (ICircuitObject)mosfet;
        }
    }
}
