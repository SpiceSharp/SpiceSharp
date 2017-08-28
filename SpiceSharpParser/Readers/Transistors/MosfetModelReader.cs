using System;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parameters;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read MOSFET models
    /// </summary>
    public class MosfetModelReader : Reader
    {
        /// <summary>
        /// Available functions for transistor levels
        /// </summary>
        public Dictionary<int, Func<string, string, string, ICircuitObject>> Levels { get; } = new Dictionary<int, Func<string, string, string, ICircuitObject>>();

        /// <summary>
        /// Constructor
        /// </summary>
        public MosfetModelReader()
            : base(StatementType.Model)
        {
            Identifier = "nmos;pmos";

            // Default MOS levels
            Levels.Add(1, (string name, string type, string version) =>
            {
                var m = new MOS1Model(name);
                switch (type)
                {
                    case "nmos": m.SetNMOS(true); break;
                    case "pmos": m.SetPMOS(true); break;
                }
                return m;
            });
            Levels.Add(2, (string name, string type, string version) =>
            {
                var m = new MOS2Model(name);
                switch (type)
                {
                    case "nmos": m.SetNMOS(true); break;
                    case "pmos": m.SetPMOS(true); break;
                }
                return m;
            });
            Levels.Add(3, (string name, string type, string version) =>
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
                        break;
                    }
                    if (at.Name.image.ToLower() == "version")
                    {
                        vindex = i;
                        version = at.Value.image.ToLower();
                    }
                }
            }
            if (lindex >= 0)
                st.Parameters.RemoveAt(lindex);
            if (vindex >= 0)
                st.Parameters.RemoveAt(vindex < lindex ? vindex : vindex - 1);

            // Generate the model
            ICircuitObject model = null;
            if (Levels.ContainsKey(level))
                model = Levels[level].Invoke(st.Name.image.ToLower(), type, version);
            else
                throw new ParseException(st.Name, $"Unknown mosfet model level {level}");

            // Output
            netlist.Path.Objects.Add(model);
            Generated = model;
            return true;
        }
    }
}
