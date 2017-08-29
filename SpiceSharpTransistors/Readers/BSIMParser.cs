using System;
using System.Collections.Generic;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class contains a standard method for parsing BSIM models
    /// </summary>
    public static class BSIMParser
    {
        /// <summary>
        /// Mosfet generators for BSIM transistors
        /// </summary>
        public static Dictionary<Type, Func<string, ICircuitObject, ICircuitComponent>> Mosfets { get; } = new Dictionary<Type, Func<string, ICircuitObject, ICircuitComponent>>()
        {
            {  typeof(BSIM1Model), (string name, ICircuitObject model) =>
            {
                BSIM1 m = new BSIM1(name);
                m.SetModel((BSIM1Model)model);
                return m;
            } },
            { typeof(BSIM2Model), (string name, ICircuitObject model) =>
            {
                BSIM2 m = new BSIM2(name);
                m.SetModel((BSIM2Model)model);
                return m;
            } },
            { typeof(BSIM3v24Model), (string name, ICircuitObject model) =>
            {
                BSIM3v24 m = new BSIM3v24(name);
                m.SetModel((BSIM3v24Model)model);
                return m;
            } },
            { typeof(BSIM3v30Model), (string name, ICircuitObject model) =>
            {
                BSIM3v30 m = new BSIM3v30(name);
                m.SetModel((BSIM3v30Model)model);
                return m;
            } },
            { typeof(BSIM4v80Model), (string name, ICircuitObject model) =>
            {
                BSIM4v80 m = new BSIM4v80(name);
                m.SetModel((BSIM4v80Model)model);
                return m;
            } }
        };

        /// <summary>
        /// Mosfet model generators
        /// </summary>
        public static Dictionary<int, Func<string, string, string, ICircuitObject>> Levels { get; } = new Dictionary<int, Func<string, string, string, ICircuitObject>>()
        {
            { 4, GenerateBSIM1Model },
            { 5, GenerateBSIM2Model },
            { 7, GenerateBSIM3Model },
            { 14, GenerateBSIM4Model }
        };

        /// <summary>
        /// Add mofset generators
        /// </summary>
        /// <param name="mosfets">The list of mosfet generators</param>
        public static void AddMosfetGenerators(Dictionary<Type, Func<string, ICircuitObject, ICircuitComponent>> mosfets)
        {
            foreach (var m in Mosfets)
                mosfets.Add(m.Key, m.Value);
        }

        /// <summary>
        /// Add mosfet model levels
        /// </summary>
        /// <param name="levels">The list of levels</param>
        public static void AddMosfetModelGenerators(Dictionary<int, Func<string, string, string, ICircuitObject>> levels)
        {
            foreach (var m in Levels)
                levels.Add(m.Key, m.Value);
        }

        /// <summary>
        /// BSIM1 model generator
        /// Version is ignored
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">nmos or pmos</param>
        /// <param name="version">Version</param>
        /// <returns></returns>
        public static ICircuitObject GenerateBSIM1Model(string name, string type, string version)
        {
            BSIM1Model model = new BSIM1Model(name);
            switch (type)
            {
                case "nmos": model.SetNMOS(true); break;
                case "pmos": model.SetPMOS(true); break;
                default:
                    throw new Exception("Invalid type \"" + type + "\"");
            }
            return model;
        }

        /// <summary>
        /// BSIM2 model generator
        /// Version is ignored
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">nmos or pmos</param>
        /// <param name="version">Version</param>
        /// <returns></returns>
        public static ICircuitObject GenerateBSIM2Model(string name, string type, string version)
        {
            BSIM2Model model = new BSIM2Model(name);
            switch (type)
            {
                case "nmos": model.SetNMOS(true); break;
                case "pmos": model.SetPMOS(true); break;
                default:
                    throw new Exception("Invalid type \"" + type + "\"");
            }
            return model;
        }

        /// <summary>
        /// Generate a BSIM3 model
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="type">Type of the parameter</param>
        /// <param name="version">Version of the parameter</param>
        /// <returns></returns>
        public static ICircuitObject GenerateBSIM3Model(string name, string type, string version)
        {
            double v = 3.3;
            switch (version)
            {
                case null:
                case "3.3.0": v = 3.3; break;
                case "3.2.4": v = 3.24; break;
                default:
                    if (!double.TryParse(version, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v))
                        throw new Exception("Unsupported version \"" + version + "\"");
                    break;
            }

            if (Math.Abs(v - 3.3) < 1e-12)
            {
                var b3v30 = new BSIM3v30Model(name);
                switch (type)
                {
                    case "nmos": b3v30.SetNMOS(true); break;
                    case "pmos": b3v30.SetPMOS(true); break;
                    default: throw new Exception("Invalid type \"" + type + "\"");
                }
                return b3v30;
            }
            else if (Math.Abs(v - 3.24) < 1e-12)
            {
                var b3v24 = new BSIM3v24Model(name);
                switch (type)
                {
                    case "nmos": b3v24.SetNMOS(true); break;
                    case "pmos": b3v24.SetPMOS(true); break;
                    default: throw new Exception("Invalid type \"" + type + "\"");
                }
                return b3v24;
            }
            else
                throw new Exception("Unrecognized version \"" + v.ToString() + "\"");
        }

        /// <summary>
        /// Generate a BSIM4 model
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">nmos or pmos</param>
        /// <param name="version">Version</param>
        /// <returns></returns>
        public static ICircuitObject GenerateBSIM4Model(string name, string type, string version)
        {
            double v = 4.8;
            switch (version)
            {
                case null:
                case "4.8.0": v = 4.8; break;
                default:
                    if (!double.TryParse(version, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v))
                        throw new Exception("Unsupported version \"" + version + "\"");
                    break;
            }

            if (Math.Abs(v - 4.8) < 1e-12)
            {
                BSIM4v80Model b4v80 = new BSIM4v80Model(name);
                switch (type)
                {
                    case "nmos": b4v80.SetNMOS(true); break;
                    case "pmos": b4v80.SetPMOS(true); break;
                    default: throw new Exception("Invalid type \"" + type + "\"");
                }
                return b4v80;
            }
            else
                throw new Exception("Invalid version \"" + v.ToString() + "\"");
        }
    }
}
