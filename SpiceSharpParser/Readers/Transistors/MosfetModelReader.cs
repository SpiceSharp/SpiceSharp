using System;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads Mosfet model definitions.
    /// It contains model generates indexed by the parameter LEVEL. Default implemented models are:
    /// - LEVEL 1: <see cref="MOS1Model"/>
    /// - LEVEL 2: <see cref="MOS2Model"/>
    /// - LEVEL 3: <see cref="MOS3Model"/>
    /// </summary>
    public class MosfetModelReader : Reader
    {
        /// <summary>
        /// Available model generators indexed by their LEVEL.
        /// The parameters passed are name, type (nmos or pmos) and the version.
        /// </summary>
        public Dictionary<int, Func<Identifier, string, string, Entity>> Levels { get; } = new Dictionary<int, Func<Identifier, string, string, Entity>>();

        /// <summary>
        /// Constructor
        /// </summary>
        public MosfetModelReader()
            : base(StatementType.Model)
        {
            Identifier = "nmos;pmos";

            // Default MOS levels
            Levels.Add(1, (Identifier name, string type, string version) =>
            {
                var m = new MOS1Model(name);
                switch (type)
                {
                    case "nmos": m.SetNMOS(true); break;
                    case "pmos": m.SetPMOS(true); break;
                }
                return m;
            });
            Levels.Add(2, (Identifier name, string type, string version) =>
            {
                var m = new MOS2Model(name);
                switch (type)
                {
                    case "nmos": m.SetNMOS(true); break;
                    case "pmos": m.SetPMOS(true); break;
                }
                return m;
            });
            Levels.Add(3, (Identifier name, string type, string version) =>
            {
                var m = new MOS3Model(name);
                switch (type)
                {
                    case "nmos": m.SetNMOS(true); break;
                    case "pmos": m.SetPMOS(true); break;
                }
                return m;
            });
        }

        /// <summary>
        /// Read a transistor model
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="st">Statement</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            // Errors
            switch (st.Parameters.Count)
            {
                case 0: throw new ParseException(st.Name, "Model name and type expected", false);
            }

            // The model depends on the model level, find the model statement
            int level = 0; string version = null;
            int lindex = -1, vindex = -1;
            for (int i = 0; i < st.Parameters.Count; i++)
            {
                if (st.Parameters[i].kind == ASSIGNMENT)
                {
                    AssignmentToken at = st.Parameters[i] as AssignmentToken;
                    if (at.Name.image.ToLower() == "level")
                    {
                        lindex = i;
                        level = (int)Math.Round(netlist.ParseDouble(at.Value));
                    }
                    if (at.Name.image.ToLower() == "version")
                    {
                        vindex = i;
                        version = at.Value.image.ToLower();
                    }
                    if (vindex >= 0 && lindex >= 0)
                        break;
                }
            }
            if (lindex >= 0)
                st.Parameters.RemoveAt(lindex);
            if (vindex >= 0)
                st.Parameters.RemoveAt(vindex < lindex ? vindex : vindex - 1);

            // Generate the model
            Entity model = null;
            if (Levels.ContainsKey(level))
                model = Levels[level].Invoke(new Identifier(st.Name.image), type, version);
            else
                throw new ParseException(st.Name, $"Unknown mosfet model level {level}");

            // Read all the parameters
            netlist.ReadParameters(model, st.Parameters);

            // Output
            netlist.Circuit.Objects.Add(model);
            Generated = model;
            return true;
        }
    }
}
