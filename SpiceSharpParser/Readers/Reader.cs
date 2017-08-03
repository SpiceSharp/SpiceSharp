using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharp.Parameters;

using static SpiceSharp.Parser.SpiceSharpParserConstants;

namespace SpiceSharp.Parser.Readers
{
    public abstract class Reader
    {
        /// <summary>
        /// Read a line
        /// </summary>
        /// <param name="name">The opening name/id</param>
        /// <param name="parameters">The following parameters</param>
        /// <param name="netlist">The resulting netlist</param>
        /// <returns></returns>
        public abstract bool Read(Token name, List<Object> parameters, Netlist netlist);

        /// <summary>
        /// Read the nodes for the device
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected bool TryReadNodes(CircuitComponent c, List<Object> parameters, int count, int index = 0)
        {
            if (parameters.Count < index + count)
                return false;

            // Extract the nodes
            string[] nodes = new string[count];
            for (int i = index; i < index + count; i++)
            {
                string node;
                if (TryReadIdentifier(parameters[i], out node))
                    nodes[i] = node;
                else
                    return false;
            }

            // Succeeded
            c.Connect(nodes);
            return true;
        }
        protected void ReadNodes(CircuitComponent c, List<Object> parameters, int count, int index = 0)
        {
            if (parameters.Count < index + count)
            {
                if (parameters.Count > 0)
                    throw new ParseException($"Error at line {GetBeginLine(parameters[parameters.Count - 1])}, column {GetBeginColumn(parameters[parameters.Count - 1])}: {count} nodes expected");
                throw new ParseException($"Unexpected end of line, {count} nodes expected");
            }

            // Extract the nodes
            string[] nodes = new string[count];
            for (int i = index; i < index + count; i++)
                nodes[i] = ReadIdentifier(parameters[i]);

            // Succeeded
            c.Connect(nodes);
        }

        /// <summary>
        /// Check for pure numbers
        /// </summary>
        /// <param name="value">The string</param>
        /// <returns></returns>
        private bool IsNumber(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsNumber(value[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Try to read a value
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="value">The output value</param>
        /// <returns></returns>
        protected bool TryReadValue(object o, out string value)
        {
            if (o is Token)
            {
                Token t = o as Token;

                // Read a value
                if (t.kind == VALUE)
                {
                    value = t.image;
                    return true;
                }
            }

            // Failed
            value = null;
            return false;
        }
        protected string ReadValue(object o)
        {
            string value;
            if (TryReadValue(o, out value))
                return value;
            throw new ParseException($"Error at line {GetBeginLine(o)}, column {GetBeginColumn(o)}, value expected");
        }

        /// <summary>
        /// Try to read a word
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="value">The value output</param>
        /// <returns></returns>
        protected bool TryReadWord(object o, out string value)
        {
            if (o is Token)
            {
                Token t = o as Token;

                // Read a word
                if (t.kind == WORD)
                {
                    value = t.image;
                    return true;
                }
            }

            // Failed
            value = null;
            return false;
        }
        protected string ReadWord(object o)
        {
            string value;
            if (TryReadWord(o, out value))
                return value;
            throw new ParseException($"Error at line {GetBeginLine(o)}, column {GetBeginColumn(o)}, word expected");
        }

        /// <summary>
        /// Try to read an identifier
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="value">The value</param>
        /// <returns></returns>
        protected bool TryReadIdentifier(object o, out string value)
        {
            if (o is Token)
            {
                Token t = o as Token;

                // Read a word or identifier
                if (t.kind == WORD || t.kind == IDENTIFIER)
                {
                    value = t.image;
                    return true;
                }

                // Read a value if it is pure digits
                if (t.kind == VALUE && IsNumber(t.image))
                {
                    value = t.image;
                    return true;
                }
            }

            // Failed
            value = null;
            return false;
        }
        protected string ReadIdentifier(object o)
        {
            string value;
            if (TryReadIdentifier(o, out value))
                return value;
            throw new ParseException($"Error at line {GetBeginLine(o)}, column {GetBeginColumn(o)}, identifier expected");
        }

        /// <summary>
        /// Try to read a named variable
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="name">The name output</param>
        /// <param name="value">The value output</param>
        /// <returns></returns>
        protected bool TryReadNamed(object o, out object name, out object value)
        {
            if (o is SpiceSharpParser.Named)
            {
                SpiceSharpParser.Named p = o as SpiceSharpParser.Named;
                name = p.Name;
                value = p.Value;
                return true;
            }

            // Failed
            name = null;
            value = null;
            return false;
        }
        protected bool TryReadNamed(object o, out string name, out string value)
        {
            if (o is SpiceSharpParser.Named)
            {
                SpiceSharpParser.Named p = o as SpiceSharpParser.Named;
                if (TryReadWord(p.Name, out name) && TryReadValue(p.Value, out value))
                    return true;
            }

            // Failed
            name = null;
            value = null;
            return false;
        }
        protected void ReadNamed(object o, out object name, out object value)
        {
            if (TryReadNamed(o, out name, out value))
                return;
            throw new ParseException($"Error at line {GetBeginLine(o)}, column {GetBeginColumn(o)}, named parameter expected");
        }
        protected void ReadNamed(object o, out string name, out string value)
        {
            object n, v;
            if (TryReadNamed(o, out n, out v))
            {
                name = ReadWord(n);
                value = ReadValue(v);
                return;
            }
            throw new ParseException($"Error at line {GetBeginLine(o)}, column {GetBeginColumn(o)}, named parameter expected");
        }

        /// <summary>
        /// Try to read a (quoted) string
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="value">The value</param>
        /// <returns></returns>
        protected bool TryReadString(object o, out string value)
        {
            if (o is Token)
            {
                Token t = o as Token;

                if (t.kind == STRING)
                {
                    value = t.image;
                    return true;
                }
            }

            value = null;
            return false;
        }
        protected string ReadString(object o)
        {
            string value;
            if (TryReadString(o, out value))
                return value;
            throw new ParseException($"Error at line {GetBeginLine(o)}, column {GetBeginColumn(o)}, string expected");
        }

        /// <summary>
        /// Read a model
        /// </summary>
        /// <param name="o"></param>
        /// <param name="netlist"></param>
        protected bool TryReadModel<T>(object o, out T model, Netlist netlist) where T : CircuitModel
        {
            model = null;

            // Get the model name
            string name;
            if (!TryReadIdentifier(o, out name))
                return false;

            // Find the model in the circuit
            if (!netlist.Circuit.Components.Contains(name))
                return false;
            CircuitModel m = netlist.Circuit.Components[name] as CircuitModel;
            if (m == null)
                return false;

            // Cast the model
            if (m is T)
            {
                model = (T)m;
                return true;
            }
            else
                return false;
        }
        protected T ReadModel<T>(object o, Netlist netlist) where T : CircuitModel
        {
            // Get the model name
            string name = ReadIdentifier(o);

            // Find the model in the circuit
            if (!netlist.Circuit.Components.Contains(name))
                throw new ParseException($"Error at line {GetBeginLine(o)}, column {GetBeginColumn(o)}, cannot find model {GetImage(o)}");
            CircuitModel m = netlist.Circuit.Components[name] as CircuitModel;
            if (m == null)
                throw new ParseException($"Error at line {GetBeginLine(o)}, column {GetBeginColumn(o)}, the component {GetImage(o)} is not a model");

            // Cast the model
            if (m is T)
                return (T)m;
            else
                throw new ParseException($"Error at line {GetBeginLine(o)}, column {GetBeginColumn(o)}, the model {GetImage(o)} is not a {typeof(T).ToString()}");
        }

        /// <summary>
        /// Read a literal (case insensitive)
        /// </summary>
        /// <param name="o"></param>
        /// <param name="lit"></param>
        /// <returns></returns>
        protected bool TryReadLiteral(object o, string lit)
        {
            if (o is Token)
                return lit.ToLower() == (o as Token).image.ToLower();
            return false;
        }

        /// <summary>
        /// Get parameter properties
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        protected int GetBeginLine(object o)
        {
            if (o is Token)
                return (o as Token).beginLine;
            if (o is Token[])
            {
                Token[] ts = (Token[])o;
                if (ts != null && ts.Length > 0)
                    return ts[0].beginLine;
            }
            if (o is SpiceSharpParser.Named)
                return GetBeginLine((o as SpiceSharpParser.Named).Name);
            if (o is SpiceSharpParser.Bracketed)
                return GetBeginLine((o as SpiceSharpParser.Bracketed).Name);
            return -1;
        }
        protected int GetEndLine(object o)
        {
            if (o is Token)
                return (o as Token).endLine;
            if (o is Token[])
            {
                Token[] ts = (Token[])o;
                if (ts != null && ts.Length > 0)
                    return ts[ts.Length - 1].endLine;
            }
            if (o is SpiceSharpParser.Named)
                return GetEndLine((o as SpiceSharpParser.Named).Value);
            if (o is SpiceSharpParser.Bracketed)
            {
                var b = o as SpiceSharpParser.Bracketed;
                if (b.Parameters.Count == 0)
                    return GetEndLine(b.Name);
                else
                    return GetEndLine(b.Parameters.Last());
            }
            return -1;
        }
        protected int GetBeginColumn(object o)
        {
            if (o is Token)
                return (o as Token).beginColumn;
            if (o is Token[])
            {
                Token[] ts = (Token[])o;
                if (ts != null && ts.Length > 0)
                    return ts[0].beginColumn;
            }
            if (o is SpiceSharpParser.Named)
                return GetBeginColumn((o as SpiceSharpParser.Named).Name);
            if (o is SpiceSharpParser.Bracketed)
                return GetBeginColumn((o as SpiceSharpParser.Bracketed).Name);
            return -1;
        }
        protected int GetEndColumn(object o)
        {
            if (o is Token)
                return (o as Token).endColumn;
            if (o is Token[])
            {
                Token[] ts = (Token[])o;
                if (ts != null && ts.Length > 0)
                    return ts[ts.Length - 1].endColumn;
            }
            if (o is SpiceSharpParser.Named)
                return GetEndColumn((o as SpiceSharpParser.Named).Value);
            if (o is SpiceSharpParser.Bracketed)
            {
                var b = o as SpiceSharpParser.Bracketed;
                if (b.Parameters.Count == 0)
                    return GetEndColumn(b.Name);
                else
                    return GetEndColumn(b.Parameters.Last());
            }
            return -1;
        }
        protected string GetImage(object o)
        {
            if (o is Token)
                return (o as Token).image;
            if (o is SpiceSharpParser.Named)
            {
                SpiceSharpParser.Named n = o as SpiceSharpParser.Named;
                return GetImage(n.Name) + " = " + GetImage(n.Value);
            }
            if (o is SpiceSharpParser.Bracketed)
            {
                SpiceSharpParser.Bracketed b = o as SpiceSharpParser.Bracketed;
                return GetImage(b.Name) + "()";
            }
            return "";
        }

        /// <summary>
        /// Throw an exception for after a parameter
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="message">The message</param>
        protected void ThrowBefore(object parameter, string message)
        {
            throw new ParseException($"Error at line {GetBeginLine(parameter)}, column {GetBeginColumn(parameter)}: {message}");
        }

        /// <summary>
        /// Throw an exception for before a parameter
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="message"></param>
        protected void ThrowAfter(object parameter, string message)
        {
            throw new ParseException($"Error at line {GetEndLine(parameter)}, column {GetEndColumn(parameter)}: {message}");
        }

        /// <summary>
        /// Read parameters until the end and assign them to obj
        /// </summary>
        /// <param name="obj">The parameterized object</param>
        /// <param name="parameters">The parameters</param>
        /// <param name="start">The starting index</param>
        protected void ReadParameters(Parameterized obj, List<object> parameters, int start = 0)
        {
            for (int i = start; i < parameters.Count; i++)
            {
                string pname, pvalue;
                if (TryReadNamed(parameters[i], out pname, out pvalue))
                {
                    pname = pname.ToLower();
                    obj.Set(pname, pvalue);
                }
                else
                    obj.Set(ReadWord(parameters[i]));
            }
        }
    }
}
