using System;
using SpiceSharp.Components;

namespace SpiceSharp.Parser
{
    /// <summary>
    /// This class contains a standard method for parsing BSIM models
    /// </summary>
    public static class BSIMParser
    {
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
        /// BSIM1 mosfet generator
        /// Correct model type is assumed
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="model">Model</param>
        /// <returns></returns>
        public static ICircuitComponent GenerateBSIM1(string name, ICircuitObject model)
        {
            BSIM1 m = new BSIM1(name);
            m.SetModel((BSIM1Model)model);
            return m;
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
        /// BSIM2 mosfet generator
        /// Correct model type is assumed
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="model">Model</param>
        /// <returns></returns>
        public static ICircuitComponent GenerateBSIM2(string name, ICircuitObject model)
        {
            BSIM2 m = new BSIM2(name);
            m.SetModel((BSIM2Model)model);
            return m;
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
        /// Generate a BSIM3 mosfet
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="model">Model</param>
        /// <returns></returns>
        public static ICircuitComponent GenerateBSIM3(string name, ICircuitObject model)
        {
            if (model is BSIM3v30Model)
            {
                BSIM3v30 m3v3 = new BSIM3v30(name);
                m3v3.SetModel((BSIM3v30Model)model);
                return m3v3;
            }
            if (model is BSIM3v24Model)
            {
                BSIM3v24 m3v24 = new BSIM3v24(name);
                m3v24.SetModel((BSIM3v24Model)model);
                return m3v24;
            }
            throw new Exception("Invalid BSIM3 model of type \"" + model.GetType() + "\"");
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

        /// <summary>
        /// Generate a BSIM4 mosfet
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="model">Model</param>
        /// <returns></returns>
        public static ICircuitComponent GenerateBSIM4(string name, ICircuitObject model)
        {
            if (model is BSIM4v80Model)
            {
                BSIM4v80 m4v80 = new BSIM4v80(name);
                m4v80.SetModel((BSIM4v80Model)model);
                return m4v80;
            }
            throw new Exception("Invalid BSIM4 model of type \"" + model.GetType() + "\"");
        }
    }
}
