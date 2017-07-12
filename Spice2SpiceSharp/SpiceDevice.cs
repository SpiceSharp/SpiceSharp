using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spice2SpiceSharp
{
    /// <summary>
    /// A collection of all files that describe a device
    /// </summary>
    public class SpiceDevice
    {
        /// <summary>
        /// Gets the name of the device
        /// </summary>
        public string DeviceName { get; private set; }

        /// <summary>
        /// Gets the description of the device
        /// </summary>
        public string DeviceDescription { get; private set; }

        /// <summary>
        /// Enumeration of possible methods
        /// </summary>
        public enum Methods
        {
            Param, ModelParam, Load, Setup, Unsetup, PzSetup, Temperature, Trunc, FindBranch, AcLoad,
            Accept, Destroy, ModelDelete, Delete, SetIC, Ask, ModelAsk, PzLoad, Convergence, SenSetup,
            SenLoad, SenUpdate, SenAcLoad, SenPrint, SenTrunc, Distortion, Noise
        }

        /// <summary>
        /// The folder that contains all the files
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// All files in the device
        /// </summary>
        public List<string> Files { get; } = new List<string>();

        /// <summary>
        /// A list of all defined parameters
        /// Used for #ifdef/#ifndef precompiler options
        /// </summary>
        public List<string> Defined { get; } = new List<string>();

        /// <summary>
        /// The file that contains the method definitions
        /// </summary>
        public string ITF { get; set; }

        /// <summary>
        /// The definition file
        /// </summary>
        public string Def { get; set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private Dictionary<Methods, string> methods = new Dictionary<Methods, string>();

        /// <summary>
        /// Gets the variable that contains the device parameters
        /// </summary>
        public string ParameterVariable { get; private set; }

        /// <summary>
        /// Gets the variable that contains the device model parameters
        /// </summary>
        public string ModelParameterVariable { get; private set; }

        /// <summary>
        /// Make a list of all methods
        /// </summary>
        public void BuildMethodTable()
        {
            string content = null;

            // First read the ITF file and get a list of all possible methods
            Dictionary<Methods, string> methodnames = new Dictionary<Methods, string>();
            string filename = Path.Combine(Folder, ITF);
            using (var reader = new StreamReader(filename))
            {
                content = reader.ReadToEnd(); 
            }
            content = ApplyDefined(content);

            // Find the SPICEdev info structure
            var spdev = FindVariables(content, "SPICEdev");
            if (spdev.Length != 1)
                throw new Exception("Could not find SPICEdev information");
            var info = spdev[0];
            int s = info.IndexOf('{');
            int e = Code.GetMatchingParenthesis(info, s);

            // Get the device information
            int s2 = info.IndexOf('{', s + 1);
            int e2 = Code.GetMatchingParenthesis(info, s2);
            string devinfo = info.Substring(s2 + 1, e2 - s2 - 1);
            devinfo = Regex.Replace(devinfo, @"^\s+", "");
            devinfo = Regex.Replace(devinfo, @"\s+$", "");
            devinfo = Regex.Replace(Code.RemoveComments(devinfo), @"\s*,\s*", ",");
            var parameters = devinfo.Split(',');
            DeviceName = parameters[0].Trim('"');
            DeviceDescription = parameters[1].Trim('"');
            ParameterVariable = parameters[6];
            ModelParameterVariable = parameters[8];

            // The first element in the info is always some name and description of the device itself
            int s3 = info.IndexOf(',', e2 + 1);
            string methodlist = info.Substring(s3 + 1, e - s3 - 2);
            methodlist = Regex.Replace(Code.RemoveComments(methodlist), @"\s+", "");
            var names = methodlist.Split(',');
            foreach (var method in (Methods[])Enum.GetValues(typeof(Methods)))
            {
                int index = (int)method;
                if (names[index].ToLower() != "null")
                    methods.Add(method, names[index]);
            }
        }

        /// <summary>
        /// Get the method for the device
        /// </summary>
        /// <param name="method">The method to get</param>
        /// <returns></returns>
        public string GetMethod(Methods method)
        {
            string content = null;
            Regex func = new Regex(@"(\w+\*?)\s*" + methods[method] + @"\s*\([^\)]*\)[^\{]+\{");

            // Usually the name closely resembles the method, let's see if we can find it like that
            string filename = Path.Combine(Folder, methods[method].ToLower() + ".c");
            if (File.Exists(filename))
            {
                using (StreamReader sr = new StreamReader(filename))
                    content = sr.ReadToEnd();
                content = ApplyDefined(content);
                var m = func.Match(content);
                if (m.Success)
                {
                    int e = Code.GetMatchingParenthesis(content, m.Index + m.Length - 1);
                    return ApplyDefined(content.Substring(m.Index, e - m.Index + 1));
                }
            }

            // We can't find it by the file, try the other files in the folder...
            foreach (string f in Directory.GetFiles(Folder))
            {
                // Already checked...
                if (f == filename)
                    continue;

                // Read the file
                using (StreamReader sr = new StreamReader(f))
                    content = sr.ReadToEnd();
                content = ApplyDefined(content);
                var m = func.Match(content);
                if (m.Success)
                {
                    int e = Code.GetMatchingParenthesis(content, m.Index + m.Length - 1);
                    return ApplyDefined(content.Substring(m.Index, e - m.Index + 1));
                }
            }

            throw new Exception($"Could not find method '{methods[method]}'");
        }

        /// <summary>
        /// Get the name of a method
        /// </summary>
        /// <param name="method">The method to get</param>
        /// <returns></returns>
        public string GetMethodName(Methods method)
        {
            return methods[method];
        }

        /// <summary>
        /// Find a variable by a certain type and/or name
        /// It will only return the first found variable
        /// </summary>
        /// <param name="type">The declaration type</param>
        /// <param name="name">The name of the variable</param>
        /// <returns></returns>
        public string GetVariable(string type, string name = @"\w+")
        {
            // The variables called are typically stored in the [name].c file
            string content = null;
            Regex variable = new Regex(type + @"\s+" + name + @"(\[\])?\s+\=[^;]+;");

            string filename = Path.Combine(Folder, DeviceName.ToLower() + ".c");
            if (File.Exists(filename))
            {
                using (StreamReader sr = new StreamReader(filename))
                    content = sr.ReadToEnd();
                var m = variable.Match(content);
                if (m.Success)
                    return ApplyDefined(m.Value);
            }

            // We can't find it by the file, try the other files in the folder...
            foreach (string f in Directory.GetFiles(Folder))
            {
                // Already checked...
                if (f == filename)
                    continue;

                // Read the file
                using (StreamReader sr = new StreamReader(f))
                    content = sr.ReadToEnd();
                var m = variable.Match(content);
                if (m.Success)
                    return ApplyDefined(m.Value);
            }

            throw new Exception($"Could not find variable '{type} {name}'");
        }

        /// <summary>
        /// Apply defined
        /// </summary>
        /// <param name="code">The original content</param>
        /// <returns></returns>
        private string ApplyDefined(string code)
        {
            Regex ifdefstruct = new Regex(@"#ifn?def\s*(?<name>\w+)\s*(?<if>[^#]+)\s*(#else\s*(?<else>[^#]+))?#endif");

            // Ignore #define statements
            Regex definestruct = new Regex(@"#define\s*[^\r\n\\]*(\\\s*[^\r\n\\]+)*");
            code = definestruct.Replace(code, "");

            // Ignore #include statements
            Regex includestruct = new Regex(@"#include[^\r\n]+[\r\n]*");
            code = includestruct.Replace(code, "");

            bool hasreplaced = true;
            do
            {
                hasreplaced = false;
                code = ifdefstruct.Replace(code, (Match m) =>
                {
                    hasreplaced = true;

                    // Check if it is ifdef
                    if (m.Value.StartsWith("#ifdef"))
                    {
                        if (Defined.Contains(m.Groups["name"].Value))
                            return m.Groups["if"].Value;
                        else
                            return m.Groups["else"].Value;
                    }
                    else
                    {
                        if (!Defined.Contains(m.Groups["name"].Value))
                            return m.Groups["if"].Value;
                        else
                            return m.Groups["else"].Value;
                    }
                });
            } while (hasreplaced);
            return code;
        }

        /// <summary>
        /// Find the variables of a certain type in the form of TYPE variable = VALUE
        /// Does not work for strings
        /// </summary>
        /// <param name="type">The type of the declared variable</param>
        /// <returns></returns>
        private string[] FindVariables(string code, string type)
        {
            Regex fdev = new Regex(type + @"\s+(\w+)\s*\=\s*[^;]*;");
            List<string> matches = new List<string>();
            var ms = fdev.Matches(code);
            foreach (Match m in ms)
            {
                matches.Add(m.Value);
            }
            return matches.ToArray();
        }


    }
}
