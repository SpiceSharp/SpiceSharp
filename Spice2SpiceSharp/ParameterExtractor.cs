using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spice2SpiceSharp
{
    /// <summary>
    /// This class can be used to extract the parameters for a certain device
    /// </summary>
    public class ParameterExtractor
    {
        /// <summary>
        /// This class represents a single parameter
        /// </summary>
        public class DeviceParameter
        {
            /// <summary>
            /// Gets the flags
            /// </summary>
            public string Flags { get; }

            /// <summary>
            /// Get all the names
            /// </summary>
            public HashSet<string> Names { get; } = new HashSet<string>();

            /// <summary>
            /// Gets or sets the description of the parameter
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Gets the type
            /// </summary>
            public string FlagType { get; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="flags">The access flags</param>
            /// <param name="name">The name of the device</param>
            /// <param name="description">The description of the device</param>
            public DeviceParameter(string flags, string name, string description, string flagtype)
            {
                Flags = flags;
                Names.Add(name);
                Description = description;
                FlagType = flagtype;
            }
        }

        /// <summary>
        /// Get the device parameters by their ID
        /// </summary>
        public Dictionary<string, DeviceParameter> Device { get; } = new Dictionary<string, DeviceParameter>();

        /// <summary>
        /// Get the model parameters by their ID
        /// </summary>
        public Dictionary<string, DeviceParameter> Model { get; } = new Dictionary<string, DeviceParameter>();

        /// <summary>
        /// Gets the parameter ID by the model parameter name
        /// </summary>
        public Dictionary<string, string> DeviceParameters { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets the parameter ID by the device parameter name
        /// </summary>
        public Dictionary<string, string> ModelParameters { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Extract the parameters
        /// </summary>
        /// <param name="dev">The spice device</param>
        public void Extract(SpiceDevice dev)
        {
            // Get the device parameters
            string param = dev.GetVariable("IFparm", dev.ParameterVariable);
            StoreParameters(param, Device);

            // Get the model parameters
            param = dev.GetVariable("IFparm", dev.ModelParameterVariable);
            StoreParameters(param, Model);
        }

        /// <summary>
        /// Get all the parameters
        /// </summary>
        /// <param name="definition">The parameter definition</param>
        /// <returns></returns>
        private void StoreParameters(string definition, Dictionary<string, DeviceParameter> parameters)
        {
            // Remove all comments
            definition = Code.RemoveComments(definition);

            // Extract the part that lists the parameters
            int s = definition.IndexOf('{');
            int e = Code.GetMatchingParenthesis(definition, s);
            definition = definition.Substring(s + 1, e - s - 1);

            // Find each parameter
            Regex param = new Regex(@"(?<flags>\w+)\s*\(\s*""(?<name>[^""]+)""\s*,\s*(?<id>\w+)\s*,\s*(?<type>\w+)\s*,\s*""(?<description>[^""]*)""\s*\)");
            var ms = param.Matches(definition);
            foreach (Match m in ms)
            {
                // Get the values
                var id = m.Groups["id"].Value;
                var flags = m.Groups["flags"].Value;
                var name = m.Groups["name"].Value;
                var type = m.Groups["type"].Value;
                var description = m.Groups["description"].Value;

                // Store the parameter
                if (parameters.ContainsKey(id))
                    parameters[id].Names.Add(name);
                else
                {
                    // Add a new parameter to the list
                    var dp = new DeviceParameter(flags, name, description, type);
                    parameters.Add(id, dp);
                }
            }
        }
    }
}
